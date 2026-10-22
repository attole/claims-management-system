import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  computed,
  input,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterLink } from '@angular/router';
import { filter, finalize, Observable, switchMap } from 'rxjs';
import {
  ClaimStatus,
  DocumentUpload,
  Issue,
  LossRequest,
  Party,
  ReserveHistory,
  ReserveRequest,
  RiskObject,
  Warning,
  WriteResult,
} from '../../core/api/api.model';
import { FIELD_LENGTHS } from '../../core/api/api-limits';
import { AuthService } from '../../core/auth/auth.service';
import { isRefusal, readErrorBody } from '../../core/http/error-body';
import { NotifyService } from '../../core/notify/notify.service';
import { partyName, PartyValue } from '../../shared/forms/party-form';
import { RiskObjectValue } from '../../shared/forms/risk-object-form';
import { enumLabel, EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { formatMoney } from '../../shared/pipes/money.pipe';
import { confirmWith } from '../../shared/ui/confirm-dialog';
import { promptWith } from '../../shared/ui/prompt-dialog';
import { allowedNextStatuses } from '../../shared/util/action-gates';
import { AuditTab } from './audit-tab';
import { ClaimDetailStore } from './claim-detail.store';
import { ClaimHeader } from './claim-header';
import { DocumentsTab, NO_UPLOAD } from './documents-tab';
import { askPolicyToLink } from './link-policy-dialog';
import { OverviewTab } from './overview-tab';
import { PartiesTab } from './parties-tab';
import { ReservesTab } from './reserves-tab';
import { askStatusChange } from './status-change-dialog';

@Component({
  selector: 'app-claim-detail-page',
  imports: [
    RouterLink,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    EnumLabelPipe,
    ClaimHeader,
    OverviewTab,
    PartiesTab,
    ReservesTab,
    DocumentsTab,
    AuditTab,
  ],
  templateUrl: './claim-detail.page.html',
  styleUrl: './claim-detail.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ClaimDetailStore],
})
export class ClaimDetailPage {
  readonly claimId = input.required<string>();

  protected readonly store = inject(ClaimDetailStore);
  private readonly dialog = inject(MatDialog);
  private readonly notify = inject(NotifyService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly documentsTab = viewChild(DocumentsTab);

  protected readonly user = inject(AuthService).user;
  protected readonly nextStatuses = computed(() => allowedNextStatuses(this.store.nextStatuses(), this.user().role));

  protected readonly bannerMessages = signal<string[]>([]);
  protected readonly isAddingParty = signal(false);
  protected readonly isEditingLoss = signal(false);
  protected readonly isAddingRiskObject = signal(false);
  protected readonly isAddingReserve = signal(false);
  protected readonly uploadPercent = signal(NO_UPLOAD);

  constructor() {
    effect(() => {
      const claimId = this.claimId();
      untracked(() => this.store.claimId.set(claimId));
    });
  }

  protected changeStatus(to: ClaimStatus): void {
    const from = this.store.status();
    const change = this.store.checkStatus(to).pipe(
      switchMap((check) => askStatusChange(this.dialog, { from, to, check })),
      switchMap((request) => this.store.changeStatus(request)),
    );

    this.run(change, `Status changed to ${enumLabel(to)}`);
  }

  protected linkPolicy(): void {
    const search = {
      lossDate: this.store.detail().loss.lossDate,
      results: this.store.policyResults,
      isSearching: this.store.isSearchingPolicies,
      query: (text: string) => this.store.policyQuery.set(text),
    };
    const link = askPolicyToLink(this.dialog, search).pipe(
      finalize(() => this.store.policyQuery.set('')),
      switchMap((policy) => this.store.linkPolicy(policy)),
    );

    this.run(link, 'Policy linked');
  }

  protected saveLoss(loss: LossRequest): void {
    this.run(this.store.updateLoss(loss), 'Loss details saved', () => this.isEditingLoss.set(false));
  }

  protected addRiskObject(riskObject: RiskObjectValue): void {
    this.run(this.store.addRiskObject(riskObject), 'Risk object added', () => this.isAddingRiskObject.set(false));
  }

  protected togglePrimary(riskObject: RiskObject): void {
    const isPrimary = !riskObject.isPrimary;
    const message = isPrimary ? 'Primary risk object set' : 'Primary risk object unset';

    this.run(this.store.setPrimaryRiskObject(riskObject, isPrimary), message);
  }

  protected removeRiskObject(riskObject: RiskObject): void {
    const confirmation = {
      title: 'Remove risk object?',
      message: `${riskObject.assetDescription} stays in the history but is no longer active on the claim.`,
      details: [],
      confirmLabel: 'Remove',
    };
    const removal = confirmWith(this.dialog, confirmation).pipe(
      filter((isConfirmed) => isConfirmed),
      switchMap(() => this.store.removeRiskObject(riskObject)),
    );

    this.run(removal, 'Risk object removed');
  }

  protected saveNotes(notes: string): void {
    this.run(this.store.saveNotes(notes), 'Notes saved');
  }

  protected addParty(party: PartyValue): void {
    this.run(this.store.addParty(party), 'Party added', () => this.isAddingParty.set(false));
  }

  protected removeParty(party: Party): void {
    const confirmation = {
      title: 'Remove party?',
      message: `${partyName(party)} stays in the history but is no longer active on the claim.`,
      details: [],
      confirmLabel: 'Remove',
    };
    const removal = confirmWith(this.dialog, confirmation).pipe(
      filter((isConfirmed) => isConfirmed),
      switchMap(() => this.store.removeParty(party)),
    );

    this.run(removal, 'Party removed');
  }

  protected acknowledgeIssue(issue: Issue): void {
    this.run(this.store.acknowledgeIssue(issue), 'Issue acknowledged');
  }

  protected waiveIssue(issue: Issue): void {
    this.run(this.store.waiveIssue(issue), 'Issue waived');
  }

  protected submitReserve(reserve: ReserveRequest): void {
    this.run(this.store.submitReserve(reserve), `Reserve of ${formatMoney(reserve.amount)} submitted`, () =>
      this.isAddingReserve.set(false),
    );
  }

  protected approveReserve(history: ReserveHistory): void {
    this.run(this.store.approveReserve(history), 'Reserve approved');
  }

  protected rejectReserve(history: ReserveHistory): void {
    const reserve = `${formatMoney(history.amount)} ${enumLabel(history.componentType)}`;
    const prompt = {
      title: 'Reject reserve?',
      message: `The ${reserve} reserve is rejected and stays in the history.`,
      label: 'Rejection reason',
      confirmLabel: 'Reject',
      maxLength: FIELD_LENGTHS.rejectionReason,
    };
    const rejection = promptWith(this.dialog, prompt).pipe(
      switchMap((reason) => this.store.rejectReserve(history, reason)),
    );

    this.run(rejection, 'Reserve rejected');
  }

  protected retractReserve(history: ReserveHistory): void {
    const confirmation = {
      title: 'Retract reserve?',
      message: `The pending ${formatMoney(history.amount)} reserve is cancelled. Submit a new one to change it.`,
      details: [],
      confirmLabel: 'Retract',
    };
    const retraction = confirmWith(this.dialog, confirmation).pipe(
      filter((isConfirmed) => isConfirmed),
      switchMap(() => this.store.retractReserve(history)),
    );

    this.run(retraction, 'Reserve retracted');
  }

  protected retryPosting(history: ReserveHistory): void {
    this.run(this.store.retryPosting(history), 'GL posting queued again');
  }

  protected uploadDocument(upload: DocumentUpload): void {
    this.uploadPercent.set(0);
    this.store
      .uploadDocument(upload)
      .pipe(
        finalize(() => this.uploadPercent.set(NO_UPLOAD)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (progress) => {
          if (progress.kind === 'progress') {
            this.uploadPercent.set(progress.percent);
          } else {
            this.succeeded(`Document ${progress.result.data.documentName} uploaded`, progress.result.warnings);
            this.documentsTab()?.reset();
          }
        },
        error: (error: unknown) => this.refused(error),
      });
  }

  protected copied(isCopied: boolean): void {
    if (isCopied) {
      this.notify.success('Claim number copied.');
    }
  }

  private run<TData>(write: Observable<WriteResult<TData>>, message: string, then = (): void => undefined): void {
    write.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.succeeded(message, result.warnings);
        then();
      },
      error: (error: unknown) => this.refused(error),
    });
  }

  private succeeded(message: string, warnings: Warning[]): void {
    this.bannerMessages.set([]);
    this.notify.success(`${message}${warningNote(warnings)}.`);
  }

  private refused(error: unknown): void {
    if (isRefusal(error)) {
      const body = readErrorBody(error);
      const messages = Object.values(body.errors).flat();
      this.bannerMessages.set(messages.length > 0 ? messages : [body.title]);
    }
  }
}

function warningNote(warnings: Warning[]): string {
  return warnings.length > 0 ? ` with ${warnings.length} warning(s)` : '';
}
