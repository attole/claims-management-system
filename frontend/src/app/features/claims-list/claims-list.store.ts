import { computed, effect, inject, Injectable, signal, untracked } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Params, Router } from '@angular/router';
import { isCalendarDate, NO_DATE } from '../../core/api/api-dates';
import { guidOrEmpty } from '../../core/api/api-ids';
import { ClaimsFilter, ClaimStatus, ClaimSummary, PagedResult } from '../../core/api/api.model';
import { ClaimsService, DEFAULT_PAGE_SIZE } from '../../core/api/claims.service';
import { ReferenceService } from '../../core/api/reference.service';

export const CLAIM_STATUSES: readonly ClaimStatus[] = [
  'Draft',
  'Open',
  'UnderInvestigation',
  'PendingPayment',
  'Closed',
  'Reopened',
  'Withdrawn',
];

export const PAGE_SIZES: readonly number[] = [10, 25, 50, 100];

const EMPTY_PAGE: PagedResult<ClaimSummary> = {
  items: [],
  page: 1,
  pageSize: DEFAULT_PAGE_SIZE,
  totalCount: 0,
  totalPages: 0,
};

const NOT_LOADED: PagedResult<ClaimSummary> = { ...EMPTY_PAGE };

export type FilterChange = Omit<ClaimsFilter, 'page' | 'pageSize'>;

@Injectable()
export class ClaimsListStore {
  private readonly claims = inject(ClaimsService);
  private readonly reference = inject(ReferenceService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly filter = signal<ClaimsFilter>(filterFrom(this.route.snapshot.queryParamMap));

  readonly page = rxResource({
    params: () => this.filter(),
    stream: ({ params }) => this.claims.list(params),
    defaultValue: EMPTY_PAGE,
  });

  private readonly causeCodeList = rxResource({ stream: () => this.reference.causeCodes(), defaultValue: [] });

  private readonly lastPage = signal(NOT_LOADED);

  readonly hasError = computed(() => this.page.error() !== undefined);
  readonly hasLoaded = computed(() => this.lastPage() !== NOT_LOADED);
  readonly currentPage = computed(() => (this.hasLoaded() ? this.lastPage() : EMPTY_PAGE));
  readonly rows = computed(() => this.currentPage().items);
  readonly totalCount = computed(() => this.currentPage().totalCount);
  readonly isFirstLoad = computed(() => this.page.isLoading() && !this.hasLoaded());
  readonly isEmpty = computed(
    () => !this.page.isLoading() && !this.hasError() && this.hasLoaded() && this.totalCount() === 0,
  );
  readonly causeCodes = computed(() => (this.causeCodeList.hasValue() ? this.causeCodeList.value() : []));
  readonly causeNames = computed(() => new Map(this.causeCodes().map((cause) => [cause.code, cause.name])));

  constructor() {
    effect(() => {
      if (this.page.status() !== 'resolved') {
        return;
      }

      const page = this.page.value();
      const isPastTheEnd = page.items.length === 0 && page.totalCount > 0 && page.page > page.totalPages;

      untracked(() =>
        isPastTheEnd ? this.goToPage(page.totalPages, this.filter().pageSize) : this.lastPage.set(page),
      );
    });
  }

  reload(): void {
    this.page.reload();
  }

  applyFilter(change: FilterChange): void {
    this.update({ ...this.filter(), ...change, page: 1 });
  }

  goToPage(page: number, pageSize: number): void {
    const isNewSize = pageSize !== this.filter().pageSize;

    this.update({ ...this.filter(), page: isNewSize ? 1 : page, pageSize });
  }

  clear(): void {
    this.update({ ...emptyFilter(), pageSize: this.filter().pageSize });
  }

  private update(filter: ClaimsFilter): void {
    this.filter.set(filter);
    void this.router.navigate([], { relativeTo: this.route, queryParams: queryFrom(filter), replaceUrl: true });
  }
}

export function emptyFilter(): ClaimsFilter {
  return {
    statuses: [],
    dateFrom: NO_DATE,
    dateTo: NO_DATE,
    assignedHandlerId: '',
    causeOfLossCode: '',
    policyId: '',
    search: '',
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
  };
}

function filterFrom(query: ParamMap): ClaimsFilter {
  const statuses = query
    .getAll('status')
    .filter((status): status is ClaimStatus => CLAIM_STATUSES.includes(status as ClaimStatus));
  const dateFrom = query.get('dateFrom') ?? NO_DATE;
  const dateTo = query.get('dateTo') ?? NO_DATE;
  const page = Number(query.get('page') ?? 1);
  const pageSize = Number(query.get('pageSize') ?? DEFAULT_PAGE_SIZE);

  return {
    ...emptyFilter(),
    statuses,
    dateFrom: isCalendarDate(dateFrom) ? dateFrom : NO_DATE,
    dateTo: isCalendarDate(dateTo) ? dateTo : NO_DATE,
    assignedHandlerId: guidOrEmpty(query.get('assignedHandlerId') ?? ''),
    policyId: guidOrEmpty(query.get('policyId') ?? ''),
    causeOfLossCode: query.get('causeOfLossCode') ?? '',
    search: query.get('search') ?? '',
    page: Number.isInteger(page) && page > 0 ? page : 1,
    pageSize: PAGE_SIZES.includes(pageSize) ? pageSize : DEFAULT_PAGE_SIZE,
  };
}

function queryFrom(filter: ClaimsFilter): Params {
  const isDefaultPaging = filter.page === 1 && filter.pageSize === DEFAULT_PAGE_SIZE;
  const entries: [string, string | string[]][] = [
    ['status', filter.statuses],
    ['dateFrom', filter.dateFrom],
    ['dateTo', filter.dateTo],
    ['assignedHandlerId', filter.assignedHandlerId],
    ['policyId', filter.policyId],
    ['causeOfLossCode', filter.causeOfLossCode],
    ['search', filter.search],
    ['page', isDefaultPaging ? '' : String(filter.page)],
    ['pageSize', isDefaultPaging ? '' : String(filter.pageSize)],
  ];

  return Object.fromEntries(entries.filter(([, value]) => value.length > 0));
}
