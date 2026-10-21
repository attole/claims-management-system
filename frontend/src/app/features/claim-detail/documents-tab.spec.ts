import { TestBed } from '@angular/core/testing';
import { ClaimDocumentLink, DocumentUpload } from '../../core/api/api.model';
import { DEV_USERS } from '../../core/auth/dev-users';
import { CLAIM_ID } from './claim-detail.fixtures';
import { DocumentsTab } from './documents-tab';

describe('DocumentsTab (S:UI-Documents, S:DOC-MaxSize, S:DOC-MimeAllowlist)', () => {
  const link: ClaimDocumentLink = {
    downloadUrl: 'https://storage.example/report.pdf?sig=abc',
    document: {
      id: 'd1',
      claimId: CLAIM_ID,
      documentType: 'Report',
      documentName: 'plumber-report.pdf',
      contentType: 'application/pdf',
      fileSizeBytes: 1_258_291,
      notes: '',
      uploadedByUserId: DEV_USERS[0].id,
      uploadedAt: '2026-07-24T09:00:00+00:00',
      isDeleted: false,
    },
  };

  async function render(percent = -1) {
    const fixture = TestBed.createComponent(DocumentsTab);
    fixture.componentRef.setInput('documents', [link]);
    fixture.componentRef.setInput('uploadPercent', percent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const sent: DocumentUpload[] = [];
    fixture.componentInstance.uploadRequested.subscribe((upload) => sent.push(upload));
    const choose = async (file: File) => {
      const input = element.querySelector('input[type="file"]') as HTMLInputElement;
      Object.defineProperty(input, 'files', { value: { item: () => file, length: 1 }, configurable: true });
      input.dispatchEvent(new Event('change'));
      await fixture.whenStable();
      element.querySelector<HTMLButtonElement>('button[type="submit"]')?.click();
      await fixture.whenStable();
    };

    return { element, sent, choose };
  }

  it('lists documents with a download link that opens in a new tab', async () => {
    const { element } = await render();
    const download = element.querySelector('a') as HTMLAnchorElement;

    expect(element.textContent).toContain('plumber-report.pdf');
    expect(element.textContent).toContain('1.2 MB');
    expect(download.href).toBe(link.downloadUrl);
    expect(download.target).toBe('_blank');
  });

  it('uploads an accepted file', async () => {
    const { sent, choose } = await render();

    await choose(new File(['x'], 'photo.png', { type: 'image/png' }));

    expect(sent.map((upload) => upload.file.name)).toEqual(['photo.png']);
  });

  it('refuses a file type the API does not accept before uploading', async () => {
    const { element, sent, choose } = await render();

    await choose(new File(['x'], 'script.exe', { type: 'application/x-msdownload' }));

    expect(sent).toEqual([]);
    expect(element.textContent).toContain('This file type is not accepted.');
  });

  it('shows the upload progress in percent once it is known', async () => {
    const { element } = await render(40);

    expect(element.textContent).toContain('40%');
    expect(element.querySelector('mat-progress-bar')?.getAttribute('mode')).toBe('determinate');
  });

  it('shows a running bar while the upload size is not known yet', async () => {
    const { element } = await render(0);

    expect(element.textContent).not.toContain('%');
    expect(element.querySelector('mat-progress-bar')?.getAttribute('mode')).toBe('indeterminate');
  });
});
