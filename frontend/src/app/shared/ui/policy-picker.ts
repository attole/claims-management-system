import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Policy } from '../../core/api/api.model';

@Component({
  selector: 'app-policy-picker',
  imports: [ReactiveFormsModule, MatAutocompleteModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule],
  templateUrl: './policy-picker.html',
  styleUrl: './policy-picker.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PolicyPicker {
  readonly results = input.required<Policy[]>();
  readonly isSearching = input.required<boolean>();
  readonly isReadonly = input(false);

  readonly query = output<string>();
  readonly chosen = output<Policy>();

  protected readonly policySearch = new FormControl('', { nonNullable: true });

  protected search(): void {
    this.query.emit(this.policySearch.value);
  }

  protected choose(event: MatAutocompleteSelectedEvent): void {
    this.chosen.emit(event.option.value as Policy);
    this.policySearch.setValue('');
    this.query.emit('');
  }
}
