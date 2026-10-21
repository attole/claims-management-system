import { TestBed } from '@angular/core/testing';
import { PartyGroup, PartyValue } from '../forms/party-form';
import { PartyEditor } from './party-editor';

describe('PartyEditor (S:UI-Intake-Parties)', () => {
  async function render() {
    const fixture = TestBed.createComponent(PartyEditor);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const sent: PartyValue[] = [];
    fixture.componentInstance.submitted.subscribe((party) => sent.push(party));

    const submit = async (values: Partial<PartyValue>) => {
      (fixture.componentInstance as unknown as { party: PartyGroup }).party.patchValue(values);
      element.querySelector<HTMLButtonElement>('button[type="submit"]')?.click();
      await fixture.whenStable();
    };

    return { element, sent, submit };
  }

  it('sends a named party with only the name fields of its type', async () => {
    const { sent, submit } = await render();

    await submit({ type: 'Company', firstName: 'Ada', companyName: ' Harbor ' });

    expect(sent).toEqual([expect.objectContaining({ firstName: '', companyName: 'Harbor' })]);
  });

  it('sends nothing without a name and says why', async () => {
    const { element, sent, submit } = await render();

    await submit({ firstName: ' ' });

    expect(sent).toEqual([]);
    expect(element.textContent).toContain('Enter a name for the party.');
  });
});
