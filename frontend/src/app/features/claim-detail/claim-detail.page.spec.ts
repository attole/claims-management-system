import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { apiUrl } from '../../core/api/api-url';
import { NotifyService } from '../../core/notify/notify.service';
import { CLAIM_ID, CLAIMANT, claimDetail, RAISED_ISSUE, reserveHistory } from './claim-detail.fixtures';
import { ClaimDetailPage } from './claim-detail.page';

describe('ClaimDetailPage (S:UI-Detail-Header, S:UI-Detail-Tabs, S:UI-Reserves, S:UI-Documents)', () => {
  let http: HttpTestingController;
  const notify = { success: vi.fn(), error: vi.fn() };
  const emptyAudit = { items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 };

  async function render() {
    notify.success.mockClear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: NotifyService, useValue: notify },
      ],
    });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ClaimDetailPage);
    fixture.componentRef.setInput('claimId', CLAIM_ID);
    TestBed.tick();
    http.expectOne(apiUrl('reference/claim-statuses')).flush([{ status: 'Open', nextStatuses: ['Closed'] }]);
    http.expectOne(apiUrl('reference/cause-of-loss-codes')).flush([]);
    http.expectOne(apiUrl(`claims/${CLAIM_ID}`)).flush(claimDetail());
    await settle();
    http.expectOne((call) => call.url === apiUrl(`claims/${CLAIM_ID}/audit`)).flush(emptyAudit);
    await fixture.whenStable();
    const page = fixture.componentInstance as unknown as Record<string, (...args: unknown[]) => void>;
    const call = (name: string, ...args: unknown[]) => page[name].call(fixture.componentInstance, ...args);

    return { fixture, call, element: fixture.nativeElement as HTMLElement };
  }

  async function settle(): Promise<void> {
    await Promise.resolve();
    TestBed.tick();
  }

  async function expectReload(): Promise<void> {
    await settle();
    http.expectOne(apiUrl(`claims/${CLAIM_ID}`)).flush(claimDetail());
    http.expectOne((call) => call.url === apiUrl(`claims/${CLAIM_ID}/audit`)).flush(emptyAudit);
  }

  afterEach(() => http.verify());

  it('shows the header and the five tabs once the claim loads', async () => {
    const { element } = await render();
    const tabs = [...element.querySelectorAll('[role="tab"]')].map((tab) => tab.textContent?.trim());

    expect(element.querySelector('app-claim-header')?.textContent).toContain('CLM-2026-0001039');
    expect(tabs).toEqual(['Overview', 'Parties', 'Reserves', 'Documents', 'Audit Log']);
  });

  it('checks the target status, asks in the dialog, then changes it and reloads', async () => {
    const { call } = await render();
    const dialog = vi
      .spyOn(TestBed.inject(MatDialog), 'open')
      .mockReturnValue({ afterClosed: () => of({ targetStatus: 'Closed', note: 'Done.' }) } as never);

    call('changeStatus', 'Closed');
    http
      .expectOne((request) => request.url === apiUrl(`claims/${CLAIM_ID}/status-check`))
      .flush({ errors: [], warnings: [] });
    const change = http.expectOne(apiUrl(`claims/${CLAIM_ID}/status`));
    change.flush({ data: { claimId: CLAIM_ID, previousStatus: 'Open', newStatus: 'Closed' }, warnings: [] });
    await expectReload();

    expect(dialog).toHaveBeenCalled();
    expect(change.request.body).toEqual({ targetStatus: 'Closed', note: 'Done.' });
    expect(notify.success).toHaveBeenCalledWith('Status changed to Closed.');
  });

  it('shows a refusal in the banner', async () => {
    const { fixture, call, element } = await render();

    call('acknowledgeIssue', RAISED_ISSUE);
    http
      .expectOne(apiUrl(`claims/${CLAIM_ID}/issues/${RAISED_ISSUE.id}/acknowledge`))
      .flush(
        { type: 'ValidationError', title: 'Refused', status: 422, errors: { IssueId: ['The issue is resolved.'] } },
        { status: 422, statusText: 'Unprocessable Entity' },
      );
    await fixture.whenStable();

    expect(element.querySelector('.banner')?.textContent).toContain('The issue is resolved.');
  });

  it('removes a party only after confirmation', async () => {
    const { call } = await render();
    vi.spyOn(TestBed.inject(MatDialog), 'open').mockReturnValue({ afterClosed: () => of(true) } as never);

    call('removeParty', CLAIMANT);
    http.expectOne(apiUrl(`claims/${CLAIM_ID}/parties/${CLAIMANT.id}`)).flush({ data: CLAIMANT, warnings: [] });
    await expectReload();

    expect(notify.success).toHaveBeenCalledWith('Party removed.');
  });

  it('removes nothing when the confirmation is declined', async () => {
    const { call } = await render();
    vi.spyOn(TestBed.inject(MatDialog), 'open').mockReturnValue({ afterClosed: () => of(false) } as never);

    call('removeParty', CLAIMANT);

    http.expectNone(apiUrl(`claims/${CLAIM_ID}/parties/${CLAIMANT.id}`));
    expect(notify.success).not.toHaveBeenCalled();
  });

  it('shows the error state for a claim id that is not a GUID', async () => {
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ClaimDetailPage);
    fixture.componentRef.setInput('claimId', 'abc');
    TestBed.tick();
    http.match(() => true).forEach((request) => request.flush([]));

    await fixture.whenStable();

    http.expectNone(apiUrl('claims/abc'));
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('The claim could not be loaded.');
  });

  it('rejects a reserve with the reason given in the prompt', async () => {
    const { call } = await render();
    const history = reserveHistory({});
    vi.spyOn(TestBed.inject(MatDialog), 'open').mockReturnValue({ afterClosed: () => of('Not supported.') } as never);

    call('rejectReserve', history);
    const rejection = http.expectOne(apiUrl(`claims/${CLAIM_ID}/reserves/${history.id}/reject`));
    rejection.flush({
      data: { reserveHistoryId: history.id, previousStatus: 'PendingApproval', newStatus: 'Rejected' },
      warnings: [],
    });
    await expectReload();

    expect(rejection.request.body).toEqual({ rejectionReason: 'Not supported.' });
    expect(notify.success).toHaveBeenCalledWith('Reserve rejected.');
  });

  it('uploads a document, then reloads and reports its name', async () => {
    const { call } = await render();
    const file = new File(['x'], 'photo.png', { type: 'image/png' });

    call('uploadDocument', { file, documentType: 'Photo', notes: '' });
    const upload = http.expectOne(apiUrl(`claims/${CLAIM_ID}/documents`));
    upload.flush({ data: { documentName: 'photo.png' }, warnings: [] }, { status: 201, statusText: 'Created' });
    await expectReload();

    expect((upload.request.body as FormData).get('documentType')).toBe('Photo');
    expect(notify.success).toHaveBeenCalledWith('Document photo.png uploaded.');
  });
});
