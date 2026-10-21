import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { apiUrl } from '../../core/api/api-url';
import { ClaimsListStore } from './claims-list.store';

describe('ClaimsListStore (S:UI-List, S:API-GET-claims)', () => {
  let http: HttpTestingController;
  let router: { navigate: ReturnType<typeof vi.fn> };

  function createStore(query: Record<string, string | string[]> = {}): ClaimsListStore {
    router = { navigate: vi.fn(() => Promise.resolve(true)) };
    TestBed.configureTestingModule({
      providers: [
        ClaimsListStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap(query) } } },
      ],
    });
    http = TestBed.inject(HttpTestingController);

    return TestBed.inject(ClaimsListStore);
  }

  function pageOf(page: number, totalCount: number) {
    return { items: [{ id: `c${page}` }], page, pageSize: 25, totalCount, totalPages: Math.ceil(totalCount / 25) };
  }

  function flushReference(): void {
    http.match(apiUrl('reference/cause-of-loss-codes')).forEach((request) => request.flush([]));
  }

  afterEach(() => http.verify());

  it('starts from the query string and ignores values it does not know', () => {
    const store = createStore({ status: ['Open', 'Bogus'], dateFrom: '2026-01-01', dateTo: 'not-a-date', page: '2' });

    const filter = store.filter();

    expect(filter.statuses).toEqual(['Open']);
    expect(filter.dateFrom).toBe('2026-01-01');
    expect(filter.dateTo).toBe('');
    expect(filter.page).toBe(2);
  });

  it('loads the page of the current filter and exposes its rows and count', async () => {
    const store = createStore({ status: 'Draft' });
    TestBed.tick();

    const request = http.expectOne((candidate) => candidate.url === apiUrl('claims'));
    request.flush({ items: [{ id: 'c1' }], page: 1, pageSize: 25, totalCount: 1, totalPages: 1 });
    flushReference();
    await TestBed.inject(ApplicationRef).whenStable();

    expect(request.request.params.getAll('status')).toEqual(['Draft']);
    expect(store.rows().length).toBe(1);
    expect(store.totalCount()).toBe(1);
  });

  it('a failed load shows as an error with no rows instead of throwing', async () => {
    const store = createStore();
    TestBed.tick();

    http.expectOne((candidate) => candidate.url === apiUrl('claims')).flush(null, { status: 500, statusText: 'Error' });
    flushReference();
    await TestBed.inject(ApplicationRef).whenStable();

    expect(store.hasError()).toBe(true);
    expect(store.rows()).toEqual([]);
    expect(store.isEmpty()).toBe(false);
  });

  it('keeps the shown page while the next one loads', async () => {
    const store = createStore();
    TestBed.tick();
    http.expectOne((candidate) => candidate.url === apiUrl('claims')).flush(pageOf(1, 30));
    flushReference();
    await TestBed.inject(ApplicationRef).whenStable();

    store.goToPage(2, 25);
    TestBed.tick();

    expect(store.rows().length).toBe(1);
    expect(store.totalCount()).toBe(30);
    expect(store.isFirstLoad()).toBe(false);
    http.expectOne((candidate) => candidate.url === apiUrl('claims')).flush(pageOf(2, 30));
  });

  it('try again after a failed load ends with the rows', async () => {
    const store = createStore();
    TestBed.tick();
    http.expectOne((candidate) => candidate.url === apiUrl('claims')).flush(null, { status: 500, statusText: 'Error' });
    flushReference();
    await TestBed.inject(ApplicationRef).whenStable();

    store.reload();
    TestBed.tick();
    http.expectOne((candidate) => candidate.url === apiUrl('claims')).flush(pageOf(1, 1));
    await TestBed.inject(ApplicationRef).whenStable();

    expect(store.hasError()).toBe(false);
    expect(store.rows().length).toBe(1);
  });

  it('a page past the end goes to the last page', async () => {
    const store = createStore({ page: '9' });
    TestBed.tick();

    http
      .expectOne((candidate) => candidate.url === apiUrl('claims'))
      .flush({ items: [], page: 9, pageSize: 25, totalCount: 30, totalPages: 2 });
    flushReference();
    await vi.waitFor(() => {
      TestBed.tick();
      expect(store.filter().page).toBe(2);
    });

    TestBed.tick();
    http.expectOne((candidate) => candidate.url === apiUrl('claims')).flush(pageOf(2, 30));
  });

  it('keeps a policy id and a handler id from the URL only when they are GUIDs', () => {
    const guid = 'a1000000-0000-4000-8000-000000000001';

    const store = createStore({ assignedHandlerId: 'abc', policyId: guid });

    expect(store.filter().assignedHandlerId).toBe('');
    expect(store.filter().policyId).toBe(guid);
  });

  it('applyFilter goes back to page one and writes the filter to the URL', () => {
    const store = createStore({ page: '3' });

    store.applyFilter({ ...store.filter(), search: 'CLM' });

    expect(store.filter().page).toBe(1);
    expect(router.navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { search: 'CLM' }, replaceUrl: true }),
    );
  });

  it('goToPage keeps the page, and a new page size starts again at page one', () => {
    const store = createStore();

    store.goToPage(3, 25);
    const samePageSize = store.filter().page;
    store.goToPage(3, 50);

    expect(samePageSize).toBe(3);
    expect(store.filter()).toEqual(expect.objectContaining({ page: 1, pageSize: 50 }));
  });

  it('clear drops every filter but keeps the page size', () => {
    const store = createStore({ status: 'Open', search: 'x', pageSize: '50' });

    store.clear();

    expect(store.filter()).toEqual(expect.objectContaining({ statuses: [], search: '', pageSize: 50 }));
  });
});
