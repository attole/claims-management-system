import { TestBed } from '@angular/core/testing';
import { Policy } from '../../../core/api/api.model';
import { provideAppDateAdapter } from '../../../core/dates/app-date-adapter';
import { createIntakeForm } from '../intake-form';
import { PolicyLossStep } from './policy-loss-step';

describe('PolicyLossStep (S:UI-Intake-Policy, S:UI-Intake-Steps, A:API-GET-policies-coverage)', () => {
  const policy: Policy = {
    id: 'p1',
    policyNumber: 'POL-48111',
    clientName: 'Harbor Logistics',
    effectiveDate: '2026-01-01',
    expirationDate: '2026-12-31',
    effectiveEndDate: '2026-12-31',
    status: 'Active',
    coverageTypes: ['Property', 'Liability'],
  };

  async function render() {
    TestBed.configureTestingModule({ providers: [...provideAppDateAdapter()] });
    const form = createIntakeForm(() => ['COL-WATER']);
    const fixture = TestBed.createComponent(PolicyLossStep);
    fixture.componentRef.setInput('form', form);
    fixture.componentRef.setInput('policyResults', [policy]);
    fixture.componentRef.setInput('isSearching', false);
    fixture.componentRef.setInput('causeCodes', [
      { code: 'COL-WATER', name: 'Water damage', perilCategory: 'Property' },
    ]);
    await fixture.whenStable();
    const step = fixture.componentInstance as unknown as Record<string, (...args: unknown[]) => void>;

    return { fixture, form, step, element: fixture.nativeElement as HTMLElement };
  }

  it('choosing a policy fills the form and shows the in-force badge for the loss date', async () => {
    const { fixture, form, step, element } = await render();
    form.controls.loss.patchValue({ lossDate: new Date(Date.UTC(2026, 6, 22, 9)) });

    step['choosePolicy'].call(fixture.componentInstance, policy);
    fixture.changeDetectorRef.markForCheck();
    await fixture.whenStable();

    expect(form.controls.policy.controls.selected.value).toEqual(policy);
    expect(element.querySelector('.picked')?.textContent).toContain('In force on the loss date');
    expect(element.querySelector('.picked')?.textContent).toContain('Covers Property, Liability');
  });

  it('switching on Unknown policy drops the chosen policy', async () => {
    const { fixture, form, step } = await render();
    form.controls.policy.patchValue({ selected: policy });

    step['toggleUnknown'].call(fixture.componentInstance, true);

    expect(form.controls.policy.controls.selected.value).toBeNull();
  });
});
