import { ChangeDetectionStrategy, Component, effect, input, untracked } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CauseCode } from '../../core/api/api.model';
import { ErrorMessagePipe } from '../validators/error-message.pipe';

@Component({
  selector: 'app-cause-field',
  imports: [ReactiveFormsModule, MatAutocompleteModule, MatFormFieldModule, MatInputModule, ErrorMessagePipe],
  templateUrl: './cause-field.html',
  styleUrl: './cause-field.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CauseField {
  readonly control = input.required<FormControl<string>>();
  readonly causeCodes = input.required<CauseCode[]>();

  protected readonly causeSearch = new FormControl('', { nonNullable: true });

  constructor() {
    effect(() => {
      const causes = this.causeCodes();
      const code = this.control().value;

      untracked(() => {
        const cause = causes.find((candidate) => candidate.code === code);
        this.causeSearch.setValue(cause?.name ?? code);
      });
    });
  }

  protected chooseCause(event: MatAutocompleteSelectedEvent): void {
    const cause = event.option.value as CauseCode;
    this.control().setValue(cause.code);
    this.causeSearch.setValue(cause.name);
  }

  protected typeCause(): void {
    const typed = this.causeSearch.value.trim().toLowerCase();
    const match = this.causeCodes().find(
      (cause) => cause.name.toLowerCase() === typed || cause.code.toLowerCase() === typed,
    );
    const control = this.control();
    control.setValue(match?.code ?? this.causeSearch.value.trim());
    control.markAsTouched();
  }

  protected matchingCauses(): CauseCode[] {
    const typed = this.causeSearch.value.trim().toLowerCase();

    return this.causeCodes().filter(
      (cause) => cause.name.toLowerCase().includes(typed) || cause.code.toLowerCase().includes(typed),
    );
  }
}
