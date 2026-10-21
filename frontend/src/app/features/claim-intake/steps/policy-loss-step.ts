import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTimepickerModule } from '@angular/material/timepicker';
import { MINIMUM_DESCRIPTION_LENGTH } from '../../../core/api/api-limits';
import { CauseCode, Policy } from '../../../core/api/api.model';
import { ErrorMessagePipe } from '../../../shared/validators/error-message.pipe';
import { inForce, IntakeForm, POLICY_REQUIRED_ERROR } from '../intake-form';

@Component({
  selector: 'app-policy-loss-step',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatTimepickerModule,
    ErrorMessagePipe,
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

  protected readonly policySearch = new FormControl('', { nonNullable: true });
  protected readonly causeSearch = new FormControl('', { nonNullable: true });
  protected readonly minimumDescription = MINIMUM_DESCRIPTION_LENGTH;
  protected readonly policyRequired = POLICY_REQUIRED_ERROR;
  protected readonly today = new Date();

  protected searchPolicies(): void {
    this.policyQuery.emit(this.policySearch.value);
  }

  protected choosePolicy(event: MatAutocompleteSelectedEvent): void {
    const policy = event.option.value as Policy;
    this.form().controls.policy.patchValue({ selected: policy, isUnknown: false });
    this.policySearch.setValue('');
    this.policyQuery.emit('');
  }

  protected clearPolicy(): void {
    this.form().controls.policy.controls.selected.setValue(null);
  }

  protected toggleUnknown(isUnknown: boolean): void {
    if (isUnknown) {
      this.clearPolicy();
    }
  }

  protected chooseCause(event: MatAutocompleteSelectedEvent): void {
    const cause = event.option.value as CauseCode;
    this.form().controls.loss.controls.causeOfLossCode.setValue(cause.code);
    this.causeSearch.setValue(cause.name);
  }

  protected typeCause(): void {
    const typed = this.causeSearch.value.trim().toLowerCase();
    const match = this.causeCodes().find(
      (cause) => cause.name.toLowerCase() === typed || cause.code.toLowerCase() === typed,
    );
    const control = this.form().controls.loss.controls.causeOfLossCode;
    control.setValue(match?.code ?? this.causeSearch.value.trim());
    control.markAsTouched();
  }

  protected matchingCauses(): CauseCode[] {
    const typed = this.causeSearch.value.trim().toLowerCase();

    return this.causeCodes().filter(
      (cause) => cause.name.toLowerCase().includes(typed) || cause.code.toLowerCase().includes(typed),
    );
  }

  protected inForce(): string {
    const value = this.form().getRawValue();

    return inForce(value.policy.selected, value.loss.lossDate);
  }

  protected descriptionLength(): number {
    return this.form().controls.loss.controls.description.value.trim().length;
  }
}
