import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { filter, Observable } from 'rxjs';
import { FIELD_LENGTHS } from '../../core/api/api-limits';
import { ChangeStatusRequest, ClaimStatus, StatusCheck } from '../../core/api/api.model';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { ErrorMessagePipe } from '../../shared/validators/error-message.pipe';

export interface StatusChangeData {
  from: ClaimStatus;
  to: ClaimStatus;
  check: StatusCheck;
}

const INPUT_FIELDS: readonly string[] = ['Reason', 'Note', 'IsLiabilityConfirmed'];

@Component({
  selector: 'app-status-change-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    EnumLabelPipe,
    ErrorMessagePipe,
  ],
  templateUrl: './status-change-dialog.html',
  styleUrl: './status-change-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusChangeDialog {
  protected readonly data = inject<StatusChangeData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<StatusChangeDialog, ChangeStatusRequest>>(MatDialogRef);

  protected readonly blockers = this.data.check.errors.filter((error) => !INPUT_FIELDS.includes(error.field));
  protected readonly asked = new Set(
    this.data.check.errors.map((error) => error.field).filter((field) => INPUT_FIELDS.includes(field)),
  );
  protected readonly askedMessages = this.data.check.errors.filter((error) => this.asked.has(error.field));
  protected readonly warnings = this.data.check.warnings;
  protected readonly isClosing = this.data.to === 'Closed';

  protected readonly form = new FormGroup({
    reasonCode: new FormControl('', {
      nonNullable: true,
      validators: [...requiredWhen(this.asked.has('Reason')), Validators.maxLength(FIELD_LENGTHS.reasonCode)],
    }),
    reasonText: new FormControl('', {
      nonNullable: true,
      validators: [...requiredWhen(this.asked.has('Reason')), Validators.maxLength(FIELD_LENGTHS.reasonText)],
    }),
    note: new FormControl('', { nonNullable: true, validators: requiredWhen(this.asked.has('Note')) }),
    isLiabilityConfirmed: new FormControl(false, {
      nonNullable: true,
      validators: this.asked.has('IsLiabilityConfirmed') ? Validators.requiredTrue : [],
    }),
  });

  private readonly formStatus = toSignal(this.form.statusChanges, { initialValue: this.form.status });

  protected canConfirm(): boolean {
    return this.blockers.length === 0 && this.formStatus() === 'VALID';
  }

  protected confirm(): void {
    this.form.markAllAsTouched();

    if (!this.canConfirm()) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      targetStatus: this.data.to,
      reasonCode: value.reasonCode.trim(),
      reasonText: value.reasonText.trim(),
      note: value.note.trim(),
      isLiabilityConfirmed: value.isLiabilityConfirmed,
    });
  }
}

export function askStatusChange(dialog: MatDialog, data: StatusChangeData): Observable<ChangeStatusRequest> {
  return dialog
    .open<StatusChangeDialog, StatusChangeData, ChangeStatusRequest>(StatusChangeDialog, { data, width: '520px' })
    .afterClosed()
    .pipe(filter((request): request is ChangeStatusRequest => request !== undefined));
}

function requiredWhen(isRequired: boolean) {
  return isRequired ? [Validators.required] : [];
}
