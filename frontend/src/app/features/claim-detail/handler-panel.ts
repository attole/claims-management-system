import { toSignal } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, computed, effect, input, output, untracked } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { EMPTY_ID } from '../../core/api/api-ids';
import { DevUser, handlerName } from '../../core/auth/dev-users';

@Component({
  selector: 'app-handler-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatSelectModule],
  templateUrl: './handler-panel.html',
  styleUrl: './handler-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HandlerPanel {
  readonly handlerId = input.required<string>();
  readonly handlers = input.required<readonly DevUser[]>();
  readonly isSaving = input(false);
  readonly isEditable = input(true);

  readonly assigned = output<string>();

  protected readonly handler = new FormControl('', { nonNullable: true });
  protected readonly picked = toSignal(this.handler.valueChanges, { initialValue: this.handler.value });
  protected readonly currentName = computed(() => handlerName(this.handlerId()));
  protected readonly canAssign = computed(() => {
    const picked = this.picked();
    const isChange = picked !== '' && picked !== this.handlerId();

    return isChange && this.isEditable() && !this.isSaving();
  });

  constructor() {
    effect(() => {
      const handlerId = this.handlerId();
      const saved = handlerId === EMPTY_ID ? '' : handlerId;

      untracked(() => {
        if (!this.handler.dirty || this.handler.value === saved) {
          this.handler.setValue(saved);
          this.handler.markAsPristine();
        }
      });
    });

    effect(() => {
      const isEditable = this.isEditable();

      untracked(() => (isEditable ? this.handler.enable() : this.handler.disable()));
    });
  }

  protected assign(): void {
    this.assigned.emit(this.handler.value);
  }
}
