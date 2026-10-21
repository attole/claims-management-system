import { TestBed } from '@angular/core/testing';
import { createIntakeForm, IntakeForm } from '../intake-form';
import { ReserveReviewStep } from './reserve-review-step';

describe('ReserveReviewStep (S:UI-Intake-Reserve)', () => {
  async function render() {
    const form = createIntakeForm(() => ['COL-WATER']);
    const fixture = TestBed.createComponent(ReserveReviewStep);
    fixture.componentRef.setInput('form', form);
    fixture.componentRef.setInput('value', form.getRawValue());
    fixture.componentRef.setInput('causeNames', new Map([['COL-WATER', 'Water damage']]));
    await fixture.whenStable();

    const change = async (edit: (form: IntakeForm) => void) => {
      edit(form);
      fixture.componentRef.setInput('value', form.getRawValue());
      await fixture.whenStable();
    };

    return { fixture, change, element: fixture.nativeElement as HTMLElement };
  }

  it('shows who must approve the amount typed', async () => {
    const { change, element } = await render();

    await change((form) => form.controls.reserve.patchValue({ componentType: 'Indemnity', amount: 60_000 }));

    expect(element.querySelector('app-authority-indicator')?.textContent).toContain('Supervisor approval required');
  });

  it('asks for a policy when a reserve is chosen without one, and links back to step 1', async () => {
    const { fixture, change, element } = await render();
    const steps: number[] = [];
    fixture.componentInstance.goToStep.subscribe((step) => steps.push(step));

    await change((form) => form.controls.reserve.patchValue({ componentType: 'Expense', amount: 100 }));
    element.querySelector<HTMLButtonElement>('button.link')?.click();

    expect(element.textContent).toContain('A reserve needs a linked policy');
    expect(steps).toEqual([0]);
  });

  it('reviews the latest form value, with the cause of loss by its name', async () => {
    const { change, element } = await render();

    await change((form) => form.controls.loss.patchValue({ causeOfLossCode: 'COL-WATER' }));

    expect(element.querySelector('table.review')?.textContent).toContain('Water damage');
  });
});
