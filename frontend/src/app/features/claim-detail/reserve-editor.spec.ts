import { TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { ReserveRequest } from '../../core/api/api.model';
import { INDEMNITY } from './claim-detail.fixtures';
import { ReserveEditor } from './reserve-editor';

describe('ReserveEditor (S:UI-Reserves, S:API-POST-reserves)', () => {
  async function render() {
    const fixture = TestBed.createComponent(ReserveEditor);
    fixture.componentRef.setInput('components', [INDEMNITY]);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const sent: ReserveRequest[] = [];
    fixture.componentInstance.submitted.subscribe((reserve) => sent.push(reserve));
    const form = (fixture.componentInstance as unknown as { form: FormGroup }).form;
    const submit = async (values: object) => {
      form.patchValue(values);
      await fixture.whenStable();
      element.querySelector<HTMLButtonElement>('button[type="submit"]')?.click();
      await fixture.whenStable();
    };

    return { element, sent, submit };
  }

  it('adjusts an open component by its id', async () => {
    const { sent, submit } = await render();

    await submit({ component: INDEMNITY.id, transactionType: 'Adjust', amount: 5000, changeReason: ' Quote. ' });

    expect(sent).toEqual([
      { componentId: INDEMNITY.id, transactionType: 'Adjust', amount: 5000, changeReason: 'Quote.' },
    ]);
  });

  it('opens a component that is not open yet with an Add by its type', async () => {
    const { sent, submit } = await render();

    await submit({ component: 'new:Expense', transactionType: 'Reverse', amount: 800, changeReason: 'Adjuster fee.' });

    expect(sent).toEqual([
      { componentType: 'Expense', transactionType: 'Add', amount: 800, changeReason: 'Adjuster fee.' },
    ]);
  });

  it('asks for a component and shows who must approve the amount', async () => {
    const { element, submit } = await render();

    await submit({ amount: 60_000 });

    expect(element.textContent).toContain('Supervisor approval required');
    expect(element.textContent).toContain('This field is required.');
  });
});
