import { computed, inject, Injectable, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { finalize, Observable, of, switchMap, tap, timer } from 'rxjs';
import { Claim, CreateClaimRequest, PagedResult, Policy, WriteResult } from '../../core/api/api.model';
import { ClaimsService } from '../../core/api/claims.service';
import { PoliciesService } from '../../core/api/policies.service';
import { ReferenceService } from '../../core/api/reference.service';
import { isRefusal } from '../../core/http/error-body';
import { newIdempotencyKey } from '../../core/http/idempotency';

export const POLICY_SEARCH_DELAY_MS = 300;
export const POLICY_SEARCH_MIN_LENGTH = 2;

const NO_POLICIES: PagedResult<Policy> = { items: [], page: 1, pageSize: 0, totalCount: 0, totalPages: 0 };

@Injectable()
export class IntakeStore {
  private readonly claims = inject(ClaimsService);
  private readonly policies = inject(PoliciesService);
  private readonly reference = inject(ReferenceService);
  private idempotencyKey = newIdempotencyKey();

  readonly policyQuery = signal('');
  readonly isSubmitting = signal(false);

  private readonly policySearch = rxResource({
    params: () => this.policyQuery().trim(),
    stream: ({ params }) =>
      params.length < POLICY_SEARCH_MIN_LENGTH
        ? of(NO_POLICIES)
        : timer(POLICY_SEARCH_DELAY_MS).pipe(switchMap(() => this.policies.search(params))),
    defaultValue: NO_POLICIES,
  });

  private readonly causeCodeList = rxResource({ stream: () => this.reference.causeCodes(), defaultValue: [] });

  readonly policyResults = computed(() => (this.policySearch.hasValue() ? this.policySearch.value().items : []));
  readonly isSearchingPolicies = computed(() => this.policySearch.isLoading());
  readonly causeCodes = computed(() => (this.causeCodeList.hasValue() ? this.causeCodeList.value() : []));
  readonly causeNames = computed(() => new Map(this.causeCodes().map((cause) => [cause.code, cause.name])));

  create(request: CreateClaimRequest): Observable<WriteResult<Claim>> {
    this.isSubmitting.set(true);

    return this.claims.create(request, this.idempotencyKey).pipe(
      tap({
        next: () => (this.idempotencyKey = newIdempotencyKey()),
        error: (error: unknown) => {
          if (isRefusal(error)) {
            this.idempotencyKey = newIdempotencyKey();
          }
        },
      }),
      finalize(() => this.isSubmitting.set(false)),
    );
  }
}
