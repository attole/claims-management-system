import { TestBed } from '@angular/core/testing';
import { Party } from '../../core/api/api.model';
import { CLAIMANT } from './claim-detail.fixtures';
import { PartiesTab } from './parties-tab';

describe('PartiesTab (S:UI-Detail-Tabs, S:UI-Intake-Parties, S:PARTY-LastClaimant)', () => {
  async function render(parties: Party[]) {
    const fixture = TestBed.createComponent(PartiesTab);
    fixture.componentRef.setInput('parties', parties);
    await fixture.whenStable();

    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('lists each party with its contact and state, and removes only active ones', async () => {
    const removedParty: Party = { ...CLAIMANT, id: 'p2', firstName: 'Old', isActive: false };
    const witness: Party = { ...CLAIMANT, id: 'p3', role: 'Witness', firstName: 'Wes' };
    const { fixture, element } = await render([CLAIMANT, removedParty, witness]);
    const removed: Party[] = [];
    fixture.componentInstance.removed.subscribe((party) => removed.push(party));
    const buttons = [...element.querySelectorAll<HTMLButtonElement>('button[aria-label^="Remove"]')];

    buttons.forEach((button) => button.click());

    expect(element.textContent).toContain('Maria Lopez');
    expect(element.textContent).toContain('maria@example.com');
    expect(element.textContent).toContain('Inactive');
    expect(removed).toEqual([witness]);
  });

  it('keeps Remove off for the last active claimant', async () => {
    const { element } = await render([CLAIMANT]);

    const remove = element.querySelector<HTMLButtonElement>('button[aria-label="Remove Maria Lopez"]');

    expect(remove?.disabled).toBe(true);
    expect(remove?.title).toBe('The last claimant cannot be removed');
  });

  it('turns every party change off on a claim that cannot be edited', async () => {
    const witness: Party = { ...CLAIMANT, id: 'p3', role: 'Witness', firstName: 'Wes' };
    const { fixture, element } = await render([CLAIMANT, witness]);
    fixture.componentRef.setInput('isEditable', false);
    await fixture.whenStable();

    const disabled = [...element.querySelectorAll<HTMLButtonElement>('button')].map((button) => button.disabled);

    expect(disabled.every((isDisabled) => isDisabled)).toBe(true);
  });

  it('opens the party editor from Add party', async () => {
    const { fixture, element } = await render([]);

    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add party'))?.click();
    await fixture.whenStable();

    expect(fixture.componentInstance.isAdding()).toBe(true);
    expect(element.querySelector('app-party-editor')).not.toBeNull();
  });
});
