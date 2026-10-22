import { ChangeDetectionStrategy, Component, effect, input, output, untracked } from '@angular/core';
import { outputFromObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, filter, map } from 'rxjs';
import { NO_DATE, parseCalendarDate, toCalendarDate } from '../../core/api/api-dates';
import { FIELD_LENGTHS } from '../../core/api/api-limits';
import { CauseCode, ClaimsFilter, ClaimStatus } from '../../core/api/api.model';
import { DevUser } from '../../core/auth/dev-users';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { DATE_RANGE_ERROR, dateRangeValidator } from '../../shared/validators/date-range';
import { ErrorMessagePipe } from '../../shared/validators/error-message.pipe';
import { CLAIM_STATUSES, FilterChange } from './claims-list.store';

export const FILTER_DEBOUNCE_MS = 300;

@Component({
  selector: 'app-claims-filter-bar',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatChipsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    EnumLabelPipe,
    ErrorMessagePipe,
  ],
  templateUrl: './claims-filter-bar.html',
  styleUrl: './claims-filter-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimsFilterBar {
  readonly filter = input.required<ClaimsFilter>();
  readonly causeCodes = input.required<CauseCode[]>();
  readonly handlers = input.required<readonly DevUser[]>();

  protected readonly statuses = CLAIM_STATUSES;
  protected readonly dateRangeError = DATE_RANGE_ERROR;

  protected readonly form = new FormGroup(
    {
      statuses: new FormControl<ClaimStatus[]>([], { nonNullable: true }),
      dateFrom: new FormControl<Date | null>(null),
      dateTo: new FormControl<Date | null>(null),
      causeOfLossCode: new FormControl('', { nonNullable: true }),
      assignedHandlerId: new FormControl('', { nonNullable: true }),
      search: new FormControl('', { nonNullable: true, validators: Validators.maxLength(FIELD_LENGTHS.search) }),
    },
    { validators: dateRangeValidator('dateFrom', 'dateTo') },
  );

  readonly changed = outputFromObservable(
    this.form.valueChanges.pipe(
      debounceTime(FILTER_DEBOUNCE_MS),
      filter(() => this.form.valid),
      map(() => this.toChange()),
      filter((change) => differingFields(change, this.filter()).length > 0),
    ),
  );

  readonly cleared = output();

  constructor() {
    effect(() => {
      const current = this.filter();

      untracked(() => {
        const fields = differingFields(this.toChange(), current);
        const target = toFormValue(current);
        const patch = Object.fromEntries(fields.map((field) => [field, target[field]]));
        this.form.patchValue(patch, { emitEvent: false });
      });
    });
  }

  private toChange(): FilterChange {
    const value = this.form.getRawValue();

    return {
      statuses: value.statuses,
      dateFrom: value.dateFrom ? toCalendarDate(value.dateFrom) : NO_DATE,
      dateTo: value.dateTo ? toCalendarDate(value.dateTo) : NO_DATE,
      causeOfLossCode: value.causeOfLossCode,
      assignedHandlerId: value.assignedHandlerId,
      policyId: this.filter().policyId,
      search: value.search.trim(),
    };
  }
}

const FORM_FIELDS = ['statuses', 'dateFrom', 'dateTo', 'causeOfLossCode', 'assignedHandlerId', 'search'] as const;

type FormField = (typeof FORM_FIELDS)[number];

function differingFields(change: FilterChange, current: ClaimsFilter): FormField[] {
  return FORM_FIELDS.filter((field) => JSON.stringify(change[field]) !== JSON.stringify(current[field]));
}

function toFormValue(filter: ClaimsFilter) {
  return {
    statuses: filter.statuses,
    dateFrom: filter.dateFrom === NO_DATE ? null : parseCalendarDate(filter.dateFrom),
    dateTo: filter.dateTo === NO_DATE ? null : parseCalendarDate(filter.dateTo),
    causeOfLossCode: filter.causeOfLossCode,
    assignedHandlerId: filter.assignedHandlerId,
    search: filter.search,
  };
}
