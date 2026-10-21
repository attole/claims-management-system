import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { formatMoney } from '../../../shared/pipes/money.pipe';
import { formatClaimDate } from '../../../shared/pipes/claim-date.pipe';
import { enumLabel, EnumLabelPipe } from '../../../shared/pipes/enum-label.pipe';
import { AuthorityIndicator } from '../../../shared/ui/authority-indicator';
import { ErrorMessagePipe } from '../../../shared/validators/error-message.pipe';
import { toIsoDateTime } from '../../../core/api/api-dates';
import {
  AMOUNT_REQUIRED_ERROR,
  IntakeForm,
  IntakeValue,
  RESERVE_COMPONENTS,
  RESERVE_NEEDS_POLICY_ERROR,
} from '../intake-form';

export interface ReviewLine {
  label: string;
  value: string;
}

@Component({
  selector: 'app-reserve-review-step',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    AuthorityIndicator,
    EnumLabelPipe,
    ErrorMessagePipe,
  ],
  templateUrl: './reserve-review-step.html',
  styleUrl: './reserve-review-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReserveReviewStep {
  readonly form = input.required<IntakeForm>();
  readonly value = input.required<IntakeValue>();
  readonly causeNames = input.required<ReadonlyMap<string, string>>();

  readonly goToStep = output<number>();

  protected readonly components = RESERVE_COMPONENTS;
  protected readonly reserveNeedsPolicy = RESERVE_NEEDS_POLICY_ERROR;
  protected readonly amountRequired = AMOUNT_REQUIRED_ERROR;

  protected readonly amount = computed(() => this.value().reserve.amount ?? 0);
  protected readonly review = computed(() => reviewLines(this.value(), this.causeNames()));
}

function reviewLines(value: IntakeValue, causeNames: ReadonlyMap<string, string>): ReviewLine[] {
  const policy = value.policy.selected;
  const lossDate = value.loss.lossDate ? formatClaimDate(toIsoDateTime(value.loss.lossDate), 'dateTime') : '';
  const cause = causeNames.get(value.loss.causeOfLossCode) ?? value.loss.causeOfLossCode;
  const parties = value.parties.map((party) => {
    const name = party.type === 'Company' ? party.companyName : `${party.firstName} ${party.lastName}`.trim();

    return `${enumLabel(party.role)}: ${name}`;
  });
  const reserve = value.reserve.componentType
    ? `${enumLabel(value.reserve.componentType)} ${formatMoney(value.reserve.amount ?? 0)}`
    : 'None';

  return [
    { label: 'Policy', value: policy ? `${policy.policyNumber} - ${policy.clientName}` : 'Unknown policy' },
    { label: 'Loss date', value: lossDate },
    { label: 'Cause of loss', value: cause },
    { label: 'Description', value: value.loss.description.trim() },
    { label: 'Location', value: value.loss.location.trim() || '-' },
    {
      label: 'Estimated loss',
      value: value.loss.estimatedLossAmount === null ? '-' : formatMoney(value.loss.estimatedLossAmount),
    },
    { label: 'Parties', value: parties.join('; ') || 'None' },
    { label: 'Risk objects', value: value.riskObjects.map((item) => item.assetDescription).join('; ') || 'None' },
    { label: 'Initial reserve', value: reserve },
  ];
}
