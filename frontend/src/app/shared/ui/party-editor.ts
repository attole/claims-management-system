import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { createPartyGroup, PARTY_ROLES, PARTY_TYPES, PartyValue, trimmedParty } from '../forms/party-form';
import { EnumLabelPipe } from '../pipes/enum-label.pipe';
import { NAME_REQUIRED_ERROR } from '../validators/claim-validators';
import { ErrorMessagePipe } from '../validators/error-message.pipe';

@Component({
  selector: 'app-party-editor',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    EnumLabelPipe,
    ErrorMessagePipe,
  ],
  templateUrl: './party-editor.html',
  styleUrl: './party-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PartyEditor {
  readonly isSaving = input(false);

  readonly submitted = output<PartyValue>();
  readonly cancelled = output();

  protected readonly roles = PARTY_ROLES;
  protected readonly types = PARTY_TYPES;
  protected readonly nameRequired = NAME_REQUIRED_ERROR;
  protected readonly party = createPartyGroup();

  protected submit(): void {
    this.party.setValue(trimmedParty(this.party.getRawValue()));
    this.party.markAllAsTouched();

    if (this.party.valid) {
      this.submitted.emit(this.party.getRawValue());
    }
  }
}
