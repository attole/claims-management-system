import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { enumLabel } from '../pipes/enum-label.pipe';

@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'classes()' },
})
export class StatusBadge {
  readonly status = input.required<string>();

  protected readonly label = computed(() => enumLabel(this.status()));
  protected readonly classes = computed(() => `badge badge--${toKebab(this.status())}`);
}

function toKebab(value: string): string {
  return value.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
}
