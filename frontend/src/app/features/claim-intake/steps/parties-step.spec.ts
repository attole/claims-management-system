import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { PartyValue } from '../../../shared/forms/party-form';
import { PartyEditor } from '../../../shared/ui/party-editor';
import { createIntakeForm } from '../intake-form';
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
    const addParty = async (party: Partial<PartyValue>) => {
      await click('button', 'Add party');
      const editor = fixture.debugElement.query(By.directive(PartyEditor)).componentInstance as PartyEditor;
      editor.submitted.emit({ ...createdParty(), ...party });
      await refresh();
    };

    return { form, element, addParty, click, refresh };
  }

  function createdParty(): PartyValue {
    return { role: 'Claimant', type: 'Person', firstName: '', lastName: '', companyName: '', email: '', phone: '' };
  }

  it('shows the claimant warning until a claimant is added', async () => {
    const { form, element, addParty } = await render();
    const warningBefore = element.textContent?.includes('Add at least one claimant');

    await addParty({ role: 'Claimant', firstName: 'Ada' });

    expect(warningBefore).toBe(true);
    expect(form.controls.parties.length).toBe(1);
    expect(element.textContent?.includes('Add at least one claimant')).toBe(false);
    expect(element.querySelector('app-party-editor')).toBeNull();
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
