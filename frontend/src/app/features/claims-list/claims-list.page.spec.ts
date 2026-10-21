import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideAppDateAdapter } from '../../core/dates/app-date-adapter';
import { provideRouter, Router } from '@angular/router';
import { apiUrl } from '../../core/api/api-url';
import { ClaimsListPage } from './claims-list.page';

describe('ClaimsListPage (S:UI-List, S:UI-Badges)', () => {
  let http: HttpTestingController;

  async function renderWith(items: object[], totalCount: number) {
    TestBed.configureTestingModule({
      imports: [ClaimsListPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), ...provideAppDateAdapter()],
    });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ClaimsListPage);
    TestBed.tick();
    http
      .expectOne((request) => request.url === apiUrl('claims'))
      .flush({
        items,
        page: 1,
        pageSize: 25,
        totalCount,
        totalPages: 1,
      });
    http
      .match(apiUrl('reference/cause-of-loss-codes'))
      .forEach((request) => request.flush([{ code: 'COL-FIRE', name: 'Fire', perilCategory: 'Property' }]));
    await fixture.whenStable();

    return fixture.nativeElement as HTMLElement;
  }

  const claim = {
    id: 'c1',
    claimNumber: 'CLM-2026-0001039',
    clientName: 'Harbor Logistics',
    policyNumber: 'POL-48111',
    lossDate: '2026-07-22T09:40:00Z',
    causeOfLossCode: 'COL-FIRE',
    status: 'PendingPayment',
    totalReserves: 25257,
  };

  afterEach(() => http.verify());

  it('shows each claim with its cause name, status badge and reserve total', async () => {
    const element = await renderWith([claim], 1);

    const row = element.querySelector('tr.row');
    expect(row?.textContent).toContain('CLM-2026-0001039');
    expect(row?.textContent).toContain('Fire');
    expect(row?.querySelector('app-status-badge')?.classList).toContain('badge--pending-payment');
    expect(row?.textContent).toContain('$25,257.00');
    expect(element.querySelector('.count')?.textContent).toContain('1 claim');
  });

  it('shows the empty state when no claim matches', async () => {
    const element = await renderWith([], 0);

    expect(element.querySelector('.state')?.textContent).toContain('No claims match these filters');
    expect(element.querySelector('table')).toBeNull();
  });

  it('opens the claim detail when a row is clicked', async () => {
    const element = await renderWith([claim], 1);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    element.querySelector<HTMLElement>('tr.row')?.click();

    expect(navigate).toHaveBeenCalledWith(['/claims', 'c1']);
  });
});
