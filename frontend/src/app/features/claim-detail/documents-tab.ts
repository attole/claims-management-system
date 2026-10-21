import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ALLOWED_UPLOAD_TYPES, FIELD_LENGTHS, MAX_UPLOAD_BYTES } from '../../core/api/api-limits';
import { ClaimDocumentLink, DocumentUpload } from '../../core/api/api.model';
import { userName } from '../../core/auth/dev-users';
import { ClaimDatePipe } from '../../shared/pipes/claim-date.pipe';
import { FileSizePipe, formatFileSize } from '../../shared/pipes/file-size.pipe';
import { ErrorMessagePipe } from '../../shared/validators/error-message.pipe';

export const NO_UPLOAD = -1;

@Component({
  selector: 'app-documents-tab',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
    MatTableModule,
    ClaimDatePipe,
    ErrorMessagePipe,
    FileSizePipe,
  ],
  templateUrl: './documents-tab.html',
  styleUrl: './documents-tab.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentsTab {
  readonly documents = input.required<ClaimDocumentLink[]>();
  readonly uploadPercent = input(NO_UPLOAD);

  readonly uploadRequested = output<DocumentUpload>();

  private readonly picker = viewChild.required<ElementRef<HTMLInputElement>>('picker');

  protected readonly accept = ALLOWED_UPLOAD_TYPES.join(',');
  protected readonly columns = ['documentName', 'documentType', 'uploadedAt', 'uploadedBy', 'size', 'download'];
  protected readonly file = signal<File | undefined>(undefined);
  protected readonly fileError = signal('');
  protected readonly isUploading = computed(() => this.uploadPercent() !== NO_UPLOAD);
  protected readonly hasPercent = computed(() => this.uploadPercent() > 0);
  protected readonly rows = computed(() =>
    this.documents()
      .filter((link) => !link.document.isDeleted)
      .map((link) => ({ ...link, uploadedBy: userName(link.document.uploadedByUserId) })),
  );

  protected readonly form = new FormGroup({
    documentType: new FormControl('', {
      nonNullable: true,
      validators: Validators.maxLength(FIELD_LENGTHS.documentType),
    }),
    notes: new FormControl('', { nonNullable: true, validators: Validators.maxLength(FIELD_LENGTHS.documentNotes) }),
  });

  protected choose(event: Event): void {
    const chosen = (event.target as HTMLInputElement).files?.item(0) ?? undefined;
    this.file.set(chosen);
    this.fileError.set(chosen ? fileProblem(chosen) : '');
  }

  protected upload(): void {
    const file = this.file();
    this.form.markAllAsTouched();

    if (!file || this.fileError().length > 0 || this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.uploadRequested.emit({ file, documentType: value.documentType.trim(), notes: value.notes.trim() });
  }

  reset(): void {
    this.picker().nativeElement.value = '';
    this.file.set(undefined);
    this.form.reset();
  }
}

function fileProblem(file: File): string {
  if (!ALLOWED_UPLOAD_TYPES.includes(file.type)) {
    return 'This file type is not accepted. Use PDF, JPEG, PNG, Word, Excel, text or CSV.';
  }

  return file.size > MAX_UPLOAD_BYTES ? `The file is larger than ${formatFileSize(MAX_UPLOAD_BYTES)}.` : '';
}
