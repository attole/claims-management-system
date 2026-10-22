import { ChangeDetectionStrategy, Component, computed, input, model, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { compareDesc } from 'date-fns';
import {
  ClaimReserves,
  ReserveApprovalStatus,
  ReserveHistory,
  ReservePostingStatus,
  ReserveRequest,
  UserRole,
} from '../../core/api/api.model';
import { parseIsoDateTime } from '../../core/api/api-dates';
import { EMPTY_ID } from '../../core/api/api-ids';
import { userName } from '../../core/auth/dev-users';
import { ClaimDatePipe } from '../../shared/pipes/claim-date.pipe';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { MoneyPipe } from '../../shared/pipes/money.pipe';
import { canDecideReserve } from '../../shared/util/reserve-authority';
import { ReserveEditor } from './reserve-editor';

const APPROVAL_TONES: Readonly<Record<ReserveApprovalStatus, string>> = {
  AutoApproved: 'badge--ok',
  Approved: 'badge--ok',
  PendingApproval: 'badge--warn',
  Rejected: 'badge--bad',
  Cancelled: '',
};

const POSTING_TONES: Readonly<Record<ReservePostingStatus, string>> = {
  Posted: 'badge--ok',
  Pending: 'badge--warn',
  Failed: 'badge--bad',
  Cancelled: '',
};

@Component({
  selector: 'app-reserves-tab',
  imports: [MatButtonModule, MatTableModule, ClaimDatePipe, EnumLabelPipe, MoneyPipe, ReserveEditor],
  templateUrl: './reserves-tab.html',
  styleUrl: './reserves-tab.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReservesTab {
  readonly reserves = input.required<ClaimReserves>();
  readonly userId = input.required<string>();
  readonly role = input.required<UserRole>();
  readonly isSaving = input(false);
  readonly isEditable = input(true);
  readonly hasPolicy = input(true);
  readonly isAdding = model(false);

  readonly submitted = output<ReserveRequest>();
  readonly approved = output<ReserveHistory>();
  readonly rejected = output<ReserveHistory>();
  readonly retracted = output<ReserveHistory>();
  readonly retried = output<ReserveHistory>();

  protected readonly columns = [
    'submittedAt',
    'transactionType',
    'componentType',
    'amount',
    'approvalStatus',
    'postingStatus',
    'submittedBy',
    'reviewedBy',
    'actions',
  ];

  protected readonly cards = computed(() => {
    const history = this.reserves().history;

    return this.reserves().components.map((component) => ({
      component,
      pending: history
        .filter((entry) => entry.reserveComponentId === component.id)
        .filter((entry) => entry.approvalStatus === 'PendingApproval')
        .reduce((sum, entry) => sum + entry.amount, 0),
    }));
  });

  protected readonly rows = computed(() => {
    return [...this.reserves().history]
      .sort((first, second) => newestFirst(first, second))
      .map((entry) => {
        const isPending = entry.approvalStatus === 'PendingApproval';
        const isOwn = entry.submittedByUserId === this.userId();
        const canDecide = isPending && canDecideReserve(this.role(), entry.amount);

        return {
          entry,
          submittedBy: userName(entry.submittedByUserId),
          reviewedBy: entry.reviewedBy === EMPTY_ID ? '-' : userName(entry.reviewedBy),
          canApprove: canDecide && !isOwn,
          canReject: canDecide,
          canRetract: isPending && isOwn,
          canRetry: entry.postingStatus === 'Failed',
          approvalTone: APPROVAL_TONES[entry.approvalStatus],
          postingTone: POSTING_TONES[entry.postingStatus],
        };
      });
  });
}

function newestFirst(first: ReserveHistory, second: ReserveHistory): number {
  const byTime = compareDesc(parseIsoDateTime(first.submittedAt), parseIsoDateTime(second.submittedAt));

  return byTime !== 0 ? byTime : second.changeSequence - first.changeSequence;
}
