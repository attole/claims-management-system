import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FIELD_LENGTHS } from '../../core/api/api-limits';
import { ReserveComponent, ReserveRequest, ReserveTransactionType } from '../../core/api/api.model';
import { RESERVE_COMPONENTS, RESERVE_TRANSACTIONS } from '../../shared/forms/reserve-form';
import { enumLabel, EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { formatMoney } from '../../shared/pipes/money.pipe';
import { AuthorityIndicator } from '../../shared/ui/authority-indicator';
import { nonZero } from '../../shared/validators/claim-validators';
import { ErrorMessagePipe } from '../../shared/validators/error-message.pipe';

export interface ComponentChoice {
  key: string;
  label: string;
  request: Pick<ReserveRequest, 'componentId' | 'componentType'>;
  isNew: boolean;
}

const NEW_TRANSACTION: ReserveTransactionType = 'Add';

@Component({
  selector: 'app-reserve-editor',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    AuthorityIndicator,
    EnumLabelPipe,
    ErrorMessagePipe,
  ],
  templateUrl: './reserve-editor.html',
  styleUrl: './reserve-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReserveEditor {
  readonly components = input.required<ReserveComponent[]>();
  readonly isSaving = input(false);

  readonly submitted = output<ReserveRequest>();
  readonly cancelled = output();

  protected readonly transactions = RESERVE_TRANSACTIONS;
  protected readonly form = new FormGroup({
    component: new FormControl('', { nonNullable: true, validators: Validators.required }),
    transactionType: new FormControl<ReserveTransactionType>(NEW_TRANSACTION, { nonNullable: true }),
    amount: new FormControl<number | null>(null, [Validators.required, nonZero]),
    changeReason: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(FIELD_LENGTHS.changeReason)],
    }),
  });

  private readonly value = toSignal(this.form.valueChanges, { initialValue: this.form.getRawValue() });

  protected readonly choices = computed(() => choicesFor(this.components()));
  protected readonly chosen = computed(() => this.choices().find((choice) => choice.key === this.value().component));
  protected readonly isNew = computed(() => this.chosen()?.isNew === true);
  protected readonly amount = computed(() => this.value().amount ?? 0);

  protected submit(): void {
    this.form.markAllAsTouched();
    const chosen = this.chosen();

    if (this.form.invalid || !chosen) {
      return;
    }

    const value = this.form.getRawValue();
    this.submitted.emit({
      ...chosen.request,
      transactionType: chosen.isNew ? NEW_TRANSACTION : value.transactionType,
      amount: value.amount ?? 0,
      changeReason: value.changeReason.trim(),
    });
  }
}

function choicesFor(components: ReserveComponent[]): ComponentChoice[] {
  const active = components.filter((component) => component.state === 'Active');
  const openTypes = new Set(active.map((component) => component.componentType));
  const existing = active.map((component) => ({
    key: component.id,
    label: `${enumLabel(component.componentType)} (${formatMoney(component.approvedBalance)})`,
    request: { componentId: component.id },
    isNew: false,
  }));
  const fresh = RESERVE_COMPONENTS.filter((type) => !openTypes.has(type)).map((type) => ({
    key: `new:${type}`,
    label: `New ${enumLabel(type)} component`,
    request: { componentType: type },
    isNew: true,
  }));

  return [...existing, ...fresh];
}
