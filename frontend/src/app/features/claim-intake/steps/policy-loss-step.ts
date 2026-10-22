import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTimepickerModule } from '@angular/material/timepicker';
import { MINIMUM_DESCRIPTION_LENGTH } from '../../../core/api/api-limits';
import { CauseCode, Policy } from '../../../core/api/api.model';
import { CauseField } from '../../../shared/ui/cause-field';
import { PolicyPicker } from '../../../shared/ui/policy-picker';
import { inForce } from '../../../shared/util/in-force';
import { ErrorMessagePipe } from '../../../shared/validators/error-message.pipe';
import { IntakeForm, POLICY_REQUIRED_ERROR } from '../intake-form';

@Component({
  selector: 'app-policy-loss-step',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    MatTimepickerModule,
    CauseField,
    ErrorMessagePipe,
    PolicyPicker,
  ],
  templateUrl: './policy-loss-step.html',
  styleUrl: './policy-loss-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PolicyLossStep {
  readonly form = input.required<IntakeForm>();
  readonly policyResults = input.required<Policy[]>();
  readonly isSearching = input.required<boolean>();
  readonly causeCodes = input.required<CauseCode[]>();

  readonly policyQuery = output<string>();

  protected readonly minimumDescription = MINIMUM_DESCRIPTION_LENGTH;
  protected readonly policyRequired = POLICY_REQUIRED_ERROR;
  protected readonly today = new Date();

  protected choosePolicy(policy: Policy): void {
    this.form().controls.policy.patchValue({ selected: policy, isUnknown: false });
  }

  protected clearPolicy(): void {
    this.form().controls.policy.controls.selected.setValue(null);
  }

  protected toggleUnknown(isUnknown: boolean): void {
    if (isUnknown) {
      this.clearPolicy();
    }
  }

  protected inForce(): string {
    const value = this.form().getRawValue();

    return inForce(value.policy.selected, value.loss.lossDate);
  }

  protected descriptionLength(): number {
    return this.form().controls.loss.controls.description.value.trim().length;
  }
}
