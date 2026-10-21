import { TestBed } from '@angular/core/testing';
import { createIntakeForm, PartyGroup } from '../intake-form';
import { PartiesStep } from './parties-step';

describe('PartiesStep (S:UI-Intake-Parties)', () => {
  async function render() {
    const form = createIntakeForm(() => []);
    const fixture = TestBed.createComponent(PartiesStep);
    fixture.componentRef.setInput('form', form);
    fixture.componentRef.setInput('value', form.getRawValue());
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const refresh = async () => {
      fixture.componentRef.setInput('value', form.getRawValue());
      await fixture.whenStable();
    };
    const click = async (selector: string, text: string) => {
      const buttons = [...element.querySelectorAll<HTMLButtonElement>(selector)];
      buttons.find((button) => button.textContent?.trim() === text)?.click();
      await refresh();
    };
    const addParty = async (values: Partial<ReturnType<PartyGroup['getRawValue']>>) => {
      await click('button', 'Add party');
      (fixture.componentInstance as unknown as { party: PartyGroup }).party.patchValue(values);
      await click('form[aria-label="New party"] button', 'Add');
    };

    return { form, element, addParty, click, refresh };
  }

  it('shows the claimant warning until a claimant is added', async () => {
    const { form, element, addParty } = await render();
    const warningBefore = element.textContent?.includes('Add at least one claimant');

    await addParty({ role: 'Claimant', firstName: 'Ada' });

    expect(warningBefore).toBe(true);
    expect(form.controls.parties.length).toBe(1);
    expect(element.textContent?.includes('Add at least one claimant')).toBe(false);
  });

  it('does not add a party without a name and says why', async () => {
    const { form, element, addParty } = await render();

    await addParty({ firstName: ' ' });

    expect(form.controls.parties.length).toBe(0);
    expect(element.textContent).toContain('Enter a name for the party.');
  });

  it('a company keeps only its company name, not names typed before the type changed', async () => {
    const { form, addParty } = await render();

    await addParty({ firstName: 'Ada', lastName: 'Lovelace', type: 'Company', companyName: 'Harbor' });

    expect(form.controls.parties.at(0).getRawValue()).toEqual(
      expect.objectContaining({ firstName: '', lastName: '', companyName: 'Harbor' }),
    );
  });

  it('shows a server error on the card of the party it names', async () => {
    const { form, element, addParty, refresh } = await render();
    await addParty({ firstName: 'Ada' });

    form.controls.parties.at(0).controls.email.setErrors({ server: 'Email is refused.' });
    await refresh();

    expect(element.querySelector('.item .error')?.textContent).toContain('Email is refused.');
  });

  it('removes an added party', async () => {
    const { form, addParty, click } = await render();
    await addParty({ firstName: 'Ada' });

    await click('button[aria-label="Remove Ada"]', 'close');

    expect(form.controls.parties.length).toBe(0);
  });
});
