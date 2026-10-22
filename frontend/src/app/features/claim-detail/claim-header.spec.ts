import { TestBed } from '@angular/core/testing';
import { EMPTY_ID } from '../../core/api/api-ids';
import { ClaimStatus } from '../../core/api/api.model';
import { claimDetail } from './claim-detail.fixtures';
import { ClaimHeader } from './claim-header';

describe('ClaimHeader (S:UI-Detail-Header)', () => {
  async function render(nextStatuses: ClaimStatus[], change = {}) {
    const detail = claimDetail();
    const fixture = TestBed.createComponent(ClaimHeader);
    fixture.componentRef.setInput('detail', { ...detail, claim: { ...detail.claim, ...change } });
    fixture.componentRef.setInput('causeName', 'Water damage');
    fixture.componentRef.setInput('nextStatuses', nextStatuses);
    await fixture.whenStable();

    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('shows the number, status, policy, client, loss day, cause and handler', async () => {
    const { element } = await render(['Closed']);
    const text = element.textContent ?? '';

    expect(text).toContain('CLM-2026-0001039');
    expect(element.querySelector('app-status-badge')?.textContent).toContain('Open');
    ['POL-48111', 'Harbor Logistics', '2026-07-22', 'Water damage', 'Hana Handler'].forEach((value) =>
      expect(text).toContain(value),
    );
  });

  it('names a missing policy and handler', async () => {
    const { element } = await render([], { policyId: EMPTY_ID, assignedHandlerId: EMPTY_ID });

    expect(element.textContent).toContain('Not linked');
    expect(element.textContent).toContain('Unassigned');
  });

  it('offers Link policy only while no policy is linked', async () => {
    const linked = await render([]);
    const { fixture, element } = await render([], { policyId: EMPTY_ID });
    let requests = 0;
    fixture.componentInstance.linkPolicyRequested.subscribe(() => requests++);
    const button = () =>
      [...element.querySelectorAll('button')].find((candidate) => candidate.textContent?.trim() === 'Link policy');

    button()?.click();

    expect(linked.element.textContent).not.toContain('Link policy');
    expect(requests).toBe(1);
  });

  it('offers only the statuses the claim can move to, and none when there are none', async () => {
    const { fixture, element } = await render(['Closed', 'Withdrawn']);
    const chosen: ClaimStatus[] = [];
    fixture.componentInstance.statusChosen.subscribe((status) => chosen.push(status));
    const trigger = () => [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Change status'));

    trigger()?.click();
    await fixture.whenStable();
    const items = [...document.querySelectorAll<HTMLButtonElement>('.mat-mdc-menu-item')];
    items[1]?.click();
    fixture.componentRef.setInput('nextStatuses', []);
    await fixture.whenStable();

    expect(items.map((item) => item.textContent?.trim())).toEqual(['Closed', 'Withdrawn']);
    expect(chosen).toEqual(['Withdrawn']);
    expect(trigger()?.disabled).toBe(true);
  });
});
