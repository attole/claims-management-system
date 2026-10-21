import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { filter, Observable } from 'rxjs';
import { ErrorMessagePipe } from '../validators/error-message.pipe';

export interface PromptDialogData {
  title: string;
  message: string;
  label: string;
  confirmLabel: string;
  maxLength: number;
}

@Component({
  selector: 'app-prompt-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    ErrorMessagePipe,
  ],
  templateUrl: './prompt-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PromptDialog {
  protected readonly data = inject<PromptDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<PromptDialog, string>>(MatDialogRef);

  protected readonly answer = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(this.data.maxLength)],
  });

  protected confirm(): void {
    const answer = this.answer.value.trim();
    this.answer.setValue(answer);
    this.answer.markAsTouched();

    if (this.answer.valid) {
      this.dialogRef.close(answer);
    }
  }
}

export function promptWith(dialog: MatDialog, data: PromptDialogData): Observable<string> {
  return dialog
    .open<PromptDialog, PromptDialogData, string>(PromptDialog, { data, width: '460px' })
    .afterClosed()
    .pipe(filter((answer): answer is string => answer !== undefined));
}
