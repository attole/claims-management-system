import { TestBed } from '@angular/core/testing';
import { ReserveHistory, UserRole } from '../../core/api/api.model';
import { DEV_USERS } from '../../core/auth/dev-users';
import { INDEMNITY, reserveHistory } from './claim-detail.fixtures';
import { ReservesTab } from './reserves-tab';

describe('ReservesTab (S:UI-Reserves, A:UI-Reserves-Roles, S:BR-R-02, S:BR-R-03, S:BR-C-06)', () => {
  const pending = reserveHistory({});
  const failed = reserveHistory({
    id: 'h2',
    changeSequence: 1,
    amount: 45000,
    approvalStatus: 'AutoApproved',
    postingStatus: 'Failed',
  });

  async function render(role: UserRole, userId: string) {
    const fixture = TestBed.createComponent(ReservesTab);
    fixture.componentRef.setInput('reserves', {
      approvedTotal: 45000,
      components: [INDEMNITY],
      history: [failed, pending],
    });
    fixture.componentRef.setInput('role', role);
    fixture.componentRef.setInput('userId', userId);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const buttons = () =>
      [...element.querySelectorAll<HTMLButtonElement>('td button')].map((button) => button.textContent?.trim());

    return { fixture, element, buttons };
  }

  it('shows the balance with what is pending per component, newest transaction first', async () => {
    const { element } = await render('Handler', DEV_USERS[0].id);
    const rows = [...element.querySelectorAll('tr.mat-mdc-row')].map((row) => row.textContent ?? '');

    expect(element.querySelector('.stat')?.textContent).toContain('$45,000.00');
    expect(element.querySelector('.pending')?.textContent).toContain('$60,000.00');
    expect(rows[0]).toContain('Pending Approval');
    expect(rows[1]).toContain('Failed');
  });

  it('orders the history by submit time across components, newest first', async () => {
    const { fixture, element } = await render('Handler', DEV_USERS[0].id);
    const expense = reserveHistory({
      id: 'h3',
      reserveComponentId: 'r2',
      componentType: 'Expense',
      changeSequence: 1,
      submittedAt: '2026-08-12T10:00:00+00:00',
    });
    const latestFirst = reserveHistory({ id: 'h4', changeSequence: 3, submittedAt: '2026-08-10T10:00:00+00:00' });

    fixture.componentRef.setInput('reserves', {
      approvedTotal: 45000,
      components: [INDEMNITY],
      history: [failed, pending, latestFirst, expense],
    });
    await fixture.whenStable();

    const components = [...element.querySelectorAll('tr.mat-mdc-row td:nth-child(3)')].map((cell) =>
      cell.textContent?.trim(),
    );
    expect(components).toEqual(['Expense', 'Indemnity', 'Indemnity', 'Indemnity']);
  });

  it('lets the submitter retract, and anyone retry a failed posting, but not decide as a handler', async () => {
    const { buttons } = await render('Handler', DEV_USERS[0].id);

    expect(buttons()).toEqual(['Retract', 'Retry posting']);
  });

  it('lets a supervisor approve or reject a pending reserve', async () => {
    const { fixture, element, buttons } = await render('Supervisor', DEV_USERS[1].id);
    const approved: ReserveHistory[] = [];
    fixture.componentInstance.approved.subscribe((entry) => approved.push(entry));

    [...element.querySelectorAll<HTMLButtonElement>('td button')]
      .find((button) => button.textContent?.includes('Approve'))
      ?.click();

    expect(buttons()).toEqual(['Approve', 'Reject', 'Retry posting']);
    expect(approved).toEqual([pending]);
  });

  it('hides Approve on your own reserve, and both decisions above the supervisor limit', async () => {
    const own = await render('Supervisor', DEV_USERS[0].id);
    const { fixture, buttons } = await render('Supervisor', DEV_USERS[1].id);
    fixture.componentRef.setInput('reserves', {
      approvedTotal: 45000,
      components: [INDEMNITY],
      history: [reserveHistory({ amount: 150_000 })],
    });
    await fixture.whenStable();

    const aboveLimit = buttons();

    expect(own.buttons()).toEqual(['Reject', 'Retract', 'Retry posting']);
    expect(aboveLimit).toEqual([]);
  });

  it('turns Add reserve off without a policy, and says why', async () => {
    const { fixture, element } = await render('Handler', DEV_USERS[0].id);
    fixture.componentRef.setInput('hasPolicy', false);
    await fixture.whenStable();

    const add = [...element.querySelectorAll('button')].find((button) => button.textContent?.trim() === 'Add reserve');

    expect(add?.disabled).toBe(true);
    expect(element.textContent).toContain('Link a policy to add reserves.');
  });
});
