import { ChangeDetectionStrategy, Component, effect, input, output, untracked } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTimepickerModule } from '@angular/material/timepicker';
import { MINIMUM_DESCRIPTION_LENGTH } from '../../core/api/api-limits';
import { CauseCode, LossDetails, LossRequest } from '../../core/api/api.model';
import { createLossGroup, lossValueOf, toLossRequest } from '../../shared/forms/loss-form';
import { CauseField } from '../../shared/ui/cause-field';
import { ErrorMessagePipe } from '../../shared/validators/error-message.pipe';

@Component({
  selector: 'app-loss-editor',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatTimepickerModule,
    CauseField,
    ErrorMessagePipe,
  ],
  templateUrl: './loss-editor.html',
  styleUrl: './loss-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LossEditor {
  readonly loss = input.required<LossDetails>();
  readonly causeCodes = input.required<CauseCode[]>();
  readonly isSaving = input(false);

  readonly submitted = output<LossRequest>();
  readonly cancelled = output();

  protected readonly form = createLossGroup(() => this.causeCodes().map((cause) => cause.code));
  protected readonly minimumDescription = MINIMUM_DESCRIPTION_LENGTH;
  protected readonly today = new Date();

  constructor() {
    effect(() => {
      const saved = lossValueOf(this.loss());
      this.causeCodes();

      untracked(() => {
        if (this.form.pristine) {
          this.form.setValue(saved);
          this.form.markAllAsTouched();
        } else {
          this.form.controls.causeOfLossCode.updateValueAndValidity();
        }
      });
    });
  }

  protected descriptionLength(): number {
    return this.form.controls.description.value.trim().length;
  }

  protected submit(): void {
    this.form.markAllAsTouched();

    if (this.form.valid) {
      this.submitted.emit(toLossRequest(this.form.getRawValue()));
    }
  }
}
