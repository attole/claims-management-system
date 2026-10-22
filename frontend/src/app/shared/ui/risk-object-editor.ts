import { ChangeDetectionStrategy, Component, effect, input, output, untracked } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ASSET_TYPES, createRiskObjectGroup, RiskObjectValue, trimmedRiskObject } from '../forms/risk-object-form';
import { ErrorMessagePipe } from '../validators/error-message.pipe';

@Component({
  selector: 'app-risk-object-editor',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ErrorMessagePipe,
  ],
  templateUrl: './risk-object-editor.html',
  styleUrl: './risk-object-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RiskObjectEditor {
  readonly isSaving = input(false);
  readonly hasPrimary = input(false);
  readonly primaryName = input('');

  readonly submitted = output<RiskObjectValue>();
  readonly cancelled = output();

  protected readonly assetTypes = ASSET_TYPES;
  protected readonly riskObject = createRiskObjectGroup();

  constructor() {
    effect(() => {
      const hasPrimary = this.hasPrimary();

      untracked(() => {
        if (hasPrimary) {
          this.riskObject.controls.isPrimary.setValue(false);
        }
      });
    });
  }

  protected togglePrimary(): void {
    const isPrimary = this.riskObject.controls.isPrimary;
    isPrimary.setValue(!isPrimary.value);
  }

  protected submit(): void {
    this.riskObject.setValue(trimmedRiskObject(this.riskObject.getRawValue()));
    this.riskObject.markAllAsTouched();

    if (this.riskObject.valid) {
      this.submitted.emit(this.riskObject.getRawValue());
    }
  }
}
