import { TestBed } from '@angular/core/testing';
import { EMPTY_ID } from '../../core/api/api-ids';
import { AuditEntry, PagedResult } from '../../core/api/api.model';
import { DEV_USERS } from '../../core/auth/dev-users';
import { AuditTab } from './audit-tab';
import { AuditPaging } from './claim-detail.store';

describe('AuditTab (S:UI-Audit)', () => {
  const entry = (userId: string): AuditEntry => ({
    id: `e-${userId}`,
    claimId: 'c1',
    eventType: 'STATUS_CHANGED',
    description: 'Open to Closed',
    oldValue: 'Open',
    newValue: 'Closed',
    relatedEntityId: EMPTY_ID,
    relatedEntityType: 'Claim',
    correlationId: EMPTY_ID,
    createdAt: '2026-08-28T17:05:00+00:00',
    createdByUserId: userId,
  });

  async function render(page: PagedResult<AuditEntry>) {
    const fixture = TestBed.createComponent(AuditTab);
    fixture.componentRef.setInput('page', page);
    await fixture.whenStable();

    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('shows each entry with the name of who made it, and the system for background jobs', async () => {
    const { element } = await render({
      items: [entry(DEV_USERS[1].id), entry(EMPTY_ID)],
      page: 1,
      pageSize: 10,
      totalCount: 2,
      totalPages: 1,
    });
    const rows = [...element.querySelectorAll('tr.mat-mdc-row')].map((row) => row.textContent ?? '');

    expect(rows[0]).toContain('STATUS_CHANGED');
    expect(rows[0]).toContain('Sam Supervisor');
    expect(rows[1]).toContain('System');
  });

  it('asks for the next page by its one-based number', async () => {
    const { fixture, element } = await render({
      items: [entry(EMPTY_ID)],
      page: 1,
      pageSize: 10,
      totalCount: 30,
      totalPages: 3,
    });
    const pages: AuditPaging[] = [];
    fixture.componentInstance.pageChanged.subscribe((paging) => pages.push(paging));

    element.querySelector<HTMLButtonElement>('button.mat-mdc-paginator-navigation-next')?.click();

    expect(pages).toEqual([{ page: 2, pageSize: 10 }]);
  });
});
