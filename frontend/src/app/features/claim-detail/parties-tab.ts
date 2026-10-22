import { ChangeDetectionStrategy, Component, computed, input, model, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Party } from '../../core/api/api.model';
import { partyName, PartyValue } from '../../shared/forms/party-form';
import { isLastActiveClaimant } from '../../shared/util/action-gates';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { PartyEditor } from '../../shared/ui/party-editor';

@Component({
  selector: 'app-parties-tab',
  imports: [MatButtonModule, EnumLabelPipe, PartyEditor],
  templateUrl: './parties-tab.html',
  styleUrl: './parties-tab.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PartiesTab {
  readonly parties = input.required<Party[]>();
  readonly isSaving = input(false);
  readonly isEditable = input(true);
  readonly isAdding = model(false);

  readonly added = output<PartyValue>();
  readonly removed = output<Party>();

  protected readonly cards = computed(() =>
    this.parties().map((party) => ({
      party,
      name: partyName(party),
      contact: [party.email, party.phone].filter((value) => value.length > 0).join(', '),
      isLastClaimant: isLastActiveClaimant(party, this.parties()),
    })),
  );
}
