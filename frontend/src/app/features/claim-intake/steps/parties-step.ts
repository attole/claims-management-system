import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { createPartyGroup, PartyGroup, partyName, PartyValue } from '../../../shared/forms/party-form';
import { EnumLabelPipe } from '../../../shared/pipes/enum-label.pipe';
import { PartyEditor } from '../../../shared/ui/party-editor';
import { CLAIMANT_REQUIRED_ERROR } from '../../../shared/validators/claim-validators';
import { ErrorMessagePipe } from '../../../shared/validators/error-message.pipe';
import { ASSET_TYPES, createRiskObjectGroup, IntakeForm, IntakeValue } from '../intake-form';

@Component({
  selector: 'app-parties-step',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    EnumLabelPipe,
    ErrorMessagePipe,
    PartyEditor,
  ],
  templateUrl: './parties-step.html',
  styleUrl: './parties-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PartiesStep {
  readonly form = input.required<IntakeForm>();
  readonly value = input.required<IntakeValue>();

  protected readonly assetTypes = ASSET_TYPES;
  protected readonly claimantRequired = CLAIMANT_REQUIRED_ERROR;

  protected readonly riskObject = createRiskObjectGroup();
  protected readonly isAddingParty = signal(false);
  protected readonly isAddingRiskObject = signal(false);

  protected readonly partyCards = computed(() => {
    this.value();

    return this.form().controls.parties.controls.map((control) => ({
      control,
      name: partyName(control.getRawValue()),
      errors: errorsOf(control),
    }));
  });

  protected addParty(party: PartyValue): void {
    const added = createPartyGroup();
    added.setValue(party);
    this.form().controls.parties.push(added);
    this.isAddingParty.set(false);
  }

  protected addRiskObject(): void {
    this.riskObject.markAllAsTouched();

    if (this.riskObject.invalid) {
      return;
    }

    const added = createRiskObjectGroup();
    added.setValue(this.riskObject.getRawValue());
    this.form().controls.riskObjects.push(added);
    this.riskObject.reset();
    this.isAddingRiskObject.set(false);
  }

  protected removeParty(index: number): void {
    this.form().controls.parties.removeAt(index);
  }

  protected removeRiskObject(index: number): void {
    this.form().controls.riskObjects.removeAt(index);
  }
}

function errorsOf(party: PartyGroup): ValidationErrors | null {
  const invalidField = Object.values(party.controls).find((control) => control.invalid);

  return party.errors ?? invalidField?.errors ?? null;
}
