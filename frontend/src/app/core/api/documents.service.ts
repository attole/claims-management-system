import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { filter, map, Observable } from 'rxjs';
import { idempotencyHeaders, newIdempotencyKey } from '../http/idempotency';
import { ClaimDocument, ClaimDocumentLink, DocumentUpload, UploadProgress, WriteResult } from './api.model';
import { apiUrl } from './api-url';

export const EMPTY_UPLOAD_RESPONSE = 'The upload finished without a document in the response.';

@Injectable({ providedIn: 'root' })
export class DocumentsService {
  private readonly http = inject(HttpClient);

  list(claimId: string): Observable<ClaimDocumentLink[]> {
    return this.http.get<ClaimDocumentLink[]>(apiUrl(`claims/${claimId}/documents`));
  }

  upload(claimId: string, upload: DocumentUpload, key = newIdempotencyKey()): Observable<UploadProgress> {
    const form = new FormData();
    form.append('file', upload.file, upload.file.name);
    form.append('documentType', upload.documentType);
    form.append('notes', upload.notes);

    return this.http
      .post<WriteResult<ClaimDocument>>(apiUrl(`claims/${claimId}/documents`), form, {
        headers: idempotencyHeaders(key),
        reportProgress: true,
        observe: 'events',
      })
      .pipe(
        filter((event) => event.type === HttpEventType.UploadProgress || event.type === HttpEventType.Response),
        map((event) => toProgress(event)),
      );
  }
}

function toProgress(event: HttpEvent<WriteResult<ClaimDocument>>): UploadProgress {
  if (event.type === HttpEventType.Response) {
    if (!event.body) {
      throw new Error(EMPTY_UPLOAD_RESPONSE);
    }

    return { kind: 'done', result: event.body };
  }

  const total = event.type === HttpEventType.UploadProgress ? (event.total ?? 0) : 0;
  const loaded = event.type === HttpEventType.UploadProgress ? event.loaded : 0;

  return { kind: 'progress', percent: total > 0 ? Math.round((loaded / total) * 100) : 0 };
}
