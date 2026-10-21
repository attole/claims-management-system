import { computed, effect, inject, Injectable, signal, untracked, WritableSignal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { finalize, Observable, tap } from 'rxjs';
import { EMPTY_ID, isGuid } from '../../core/api/api-ids';
import {
  AuditEntry,
  ChangeStatusRequest,
  ClaimDetail,
  ClaimStatus,
  DocumentUpload,
  Issue,
  PagedResult,
  Party,
  PartyRequest,
  ReserveHistory,
  ReserveRequest,
  StatusCheck,
  UploadProgress,
} from '../../core/api/api.model';
import { ClaimsService } from '../../core/api/claims.service';
import { DocumentsService } from '../../core/api/documents.service';
import { IssuesService } from '../../core/api/issues.service';
import { PartiesService } from '../../core/api/parties.service';
import { ReferenceService } from '../../core/api/reference.service';
import { ReservesService } from '../../core/api/reserves.service';
import { isRefusal } from '../../core/http/error-body';
import { newIdempotencyKey } from '../../core/http/idempotency';

export const AUDIT_PAGE_SIZES: readonly number[] = [10, 25, 50];

export interface AuditPaging {
  page: number;
  pageSize: number;
}

export const EMPTY_DETAIL: ClaimDetail = {
  claim: {
    id: EMPTY_ID,
    claimNumber: '',
    state: 'Draft',
    severity: 'Unspecified',
    reportedDate: '',
    policyId: EMPTY_ID,
    assignedHandlerId: EMPTY_ID,
    notes: '',
    hasAggregateLimitOverride: false,
  },
  clientName: '',
  policyNumber: '',
  loss: {
    id: EMPTY_ID,
    claimId: EMPTY_ID,
    reportDate: '',
    lossDate: '',
    description: '',
    causeOfLossCode: '',
    location: '',
    hasEstimatedLossAmount: false,
    estimatedLossAmount: 0,
    policeReportNumber: '',
  },
  parties: [],
  riskObjects: [],
  issues: [],
  reserves: { approvedTotal: 0, components: [], history: [] },
  documents: [],
  recentAudit: [],
};

const EMPTY_AUDIT: PagedResult<AuditEntry> = {
  items: [],
  page: 1,
  pageSize: AUDIT_PAGE_SIZES[0],
  totalCount: 0,
  totalPages: 0,
};

@Injectable()
export class ClaimDetailStore {
  private readonly claims = inject(ClaimsService);
  private readonly parties = inject(PartiesService);
  private readonly issues = inject(IssuesService);
  private readonly reserves = inject(ReservesService);
  private readonly documents = inject(DocumentsService);
  private readonly reference = inject(ReferenceService);
  private readonly keys = new Map<string, string>();

  readonly claimId = signal('');
  readonly auditPaging = signal<AuditPaging>({ page: 1, pageSize: AUDIT_PAGE_SIZES[0] });
  private readonly pendingWrites = signal(0);
  private readonly pendingUploads = signal(0);

  readonly isSaving = computed(() => this.pendingWrites() > 0);
  readonly isUploading = computed(() => this.pendingUploads() > 0);

  private readonly detailResource = rxResource({
    params: () => (isGuid(this.claimId()) ? this.claimId() : undefined),
    stream: ({ params }) => this.claims.get(params),
    defaultValue: EMPTY_DETAIL,
  });

  private readonly auditResource = rxResource({
    params: () => (this.hasLoaded() ? { claimId: this.claimId(), ...this.auditPaging() } : undefined),
    stream: ({ params }) => this.claims.listAudit(params.claimId, params.page, params.pageSize),
    defaultValue: EMPTY_AUDIT,
  });

  private readonly statusList = rxResource({ stream: () => this.reference.claimStatuses(), defaultValue: [] });
  private readonly causeCodeList = rxResource({ stream: () => this.reference.causeCodes(), defaultValue: [] });
  private readonly lastAudit = signal(EMPTY_AUDIT);
  private readonly lastDetail = signal(EMPTY_DETAIL);

  readonly isLoading = computed(() => this.detailResource.isLoading());
  readonly hasError = computed(() => !isGuid(this.claimId()) || this.detailResource.error() !== undefined);
  readonly detail = computed(() => {
    const detail = this.lastDetail();

    return detail.claim.id === this.claimId() ? detail : EMPTY_DETAIL;
  });
  readonly hasLoaded = computed(() => this.detail().claim.id !== EMPTY_ID);
  readonly status = computed(() => this.detail().claim.state);
  readonly nextStatuses = computed(() => {
    const statuses = this.statusList.hasValue() ? this.statusList.value() : [];

    return statuses.find((info) => info.status === this.status())?.nextStatuses ?? [];
  });
  readonly causeName = computed(() => {
    const causes = this.causeCodeList.hasValue() ? this.causeCodeList.value() : [];
    const code = this.detail().loss.causeOfLossCode;

    return causes.find((cause) => cause.code === code)?.name ?? code;
  });
  readonly audit = this.lastAudit.asReadonly();
  readonly isAuditLoading = computed(() => this.auditResource.isLoading());
  readonly hasAuditError = computed(() => this.auditResource.error() !== undefined);

  constructor() {
    effect(() => {
      if (this.detailResource.status() === 'resolved') {
        const detail = this.detailResource.value();
        untracked(() => this.lastDetail.set(detail));
      }
    });
    effect(() => {
      if (this.auditResource.status() === 'resolved' && this.hasLoaded()) {
        const page = this.auditResource.value();
        untracked(() => this.lastAudit.set(page));
      }
    });
  }

  reload(): void {
    this.detailResource.reload();
    this.auditResource.reload();
  }

  reloadAudit(): void {
    this.auditResource.reload();
  }

  checkStatus(targetStatus: ClaimStatus): Observable<StatusCheck> {
    return this.claims.checkStatus(this.claimId(), targetStatus);
  }

  changeStatus(request: ChangeStatusRequest) {
    const action = `status:${request.targetStatus}`;

    return this.save(action, (key) => this.claims.changeStatus(this.claimId(), request, key));
  }

  saveNotes(notes: string) {
    return this.save('notes', (key) => this.claims.updateNotes(this.claimId(), notes, key));
  }

  addParty(party: PartyRequest) {
    return this.save('party', (key) => this.parties.add(this.claimId(), party, key));
  }

  removeParty(party: Party) {
    return this.save(`party:${party.id}`, (key) => this.parties.remove(this.claimId(), party.id, key));
  }

  acknowledgeIssue(issue: Issue) {
    return this.save(`issue:${issue.id}`, (key) => this.issues.acknowledge(this.claimId(), issue.id, key));
  }

  waiveIssue(issue: Issue) {
    return this.save(`issue:${issue.id}`, (key) => this.issues.waive(this.claimId(), issue.id, key));
  }

  submitReserve(reserve: ReserveRequest) {
    return this.save('reserve', (key) => this.reserves.submit(this.claimId(), reserve, key));
  }

  approveReserve(history: ReserveHistory) {
    return this.save(`reserve:${history.id}`, (key) => this.reserves.approve(this.claimId(), history.id, key));
  }

  rejectReserve(history: ReserveHistory, reason: string) {
    return this.save(`reserve:${history.id}`, (key) => this.reserves.reject(this.claimId(), history.id, reason, key));
  }

  retractReserve(history: ReserveHistory) {
    return this.save(`reserve:${history.id}`, (key) => this.reserves.retract(this.claimId(), history.id, key));
  }

  retryPosting(history: ReserveHistory) {
    return this.save(`posting:${history.id}`, (key) => this.reserves.retryPosting(this.claimId(), history.id, key));
  }

  uploadDocument(upload: DocumentUpload): Observable<UploadProgress> {
    const { name, size, lastModified } = upload.file;
    const action = `upload:${name}:${size}:${lastModified}`;

    return this.save(
      action,
      (key) => this.documents.upload(this.claimId(), upload, key),
      (progress) => progress.kind === 'done',
      this.pendingUploads,
    );
  }

  private save<TResult>(
    action: string,
    write: (key: string) => Observable<TResult>,
    isFinal: (result: TResult) => boolean = () => true,
    pending: WritableSignal<number> = this.pendingWrites,
  ): Observable<TResult> {
    const key = this.keys.get(action) ?? newIdempotencyKey();
    this.keys.set(action, key);
    pending.update((count) => count + 1);

    return write(key).pipe(
      tap({
        next: (result) => {
          if (isFinal(result)) {
            this.keys.delete(action);
            this.reload();
          }
        },
        error: (error: unknown) => {
          if (isRefusal(error)) {
            this.keys.delete(action);
          }
        },
      }),
      finalize(() => pending.update((count) => count - 1)),
    );
  }
}
