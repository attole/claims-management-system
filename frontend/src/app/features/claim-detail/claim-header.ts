import { ClipboardModule } from '@angular/cdk/clipboard';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { EMPTY_ID } from '../../core/api/api-ids';
import { ClaimDetail, ClaimStatus } from '../../core/api/api.model';
import { handlerName } from '../../core/auth/dev-users';
import { ClaimDatePipe } from '../../shared/pipes/claim-date.pipe';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { StatusBadge } from '../../shared/ui/status-badge';

export const UNKNOWN_POLICY = 'Not linked';

@Component({
  selector: 'app-claim-header',
  imports: [ClipboardModule, MatButtonModule, MatIconModule, MatMenuModule, ClaimDatePipe, EnumLabelPipe, StatusBadge],
  templateUrl: './claim-header.html',
  styleUrl: './claim-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimHeader {
  readonly detail = input.required<ClaimDetail>();
  readonly causeName = input.required<string>();
  readonly nextStatuses = input.required<ClaimStatus[]>();
  readonly isSaving = input(false);
  readonly isEditable = input(true);

  readonly statusChosen = output<ClaimStatus>();
  readonly copied = output<boolean>();
  readonly linkPolicyRequested = output();

  protected readonly isPolicyLinked = computed(() => this.detail().claim.policyId !== EMPTY_ID);

  protected readonly policyNumber = computed(() => {
    const detail = this.detail();

    return detail.claim.policyId === EMPTY_ID ? UNKNOWN_POLICY : detail.policyNumber;
  });

  protected readonly handlerName = computed(() => handlerName(this.detail().claim.assignedHandlerId));
}
