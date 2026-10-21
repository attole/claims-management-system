import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { formatMoney } from '../pipes/money.pipe';
import { AUTO_APPROVAL_LIMIT, authorityFor } from '../util/reserve-authority';

const TEXT = {
  none: 'Enter an amount to see who can approve it.',
  auto: `Auto-approved (up to ${formatMoney(AUTO_APPROVAL_LIMIT)})`,
  supervisor: 'Supervisor approval required',
  manager: 'Manager approval required',
} as const;

@Component({
  selector: 'app-authority-indicator',
  imports: [MatIconModule],
  templateUrl: './authority-indicator.html',
  styleUrl: './authority-indicator.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthorityIndicator {
  readonly amount = input.required<number>();

  protected readonly authority = computed(() => authorityFor(this.amount()));
  protected readonly text = computed(() => TEXT[this.authority()]);
  protected readonly icon = computed(() => (this.authority() === 'auto' ? 'check_circle' : 'warning'));
}
