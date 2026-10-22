import { TestBed } from '@angular/core/testing';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { Policy } from '../../core/api/api.model';
import { PolicyPicker } from './policy-picker';

describe('PolicyPicker (S:UI-Intake-Policy)', () => {
  const policy: Policy = {
    id: 'p1',
    policyNumber: 'POL-48111',
    clientName: 'Harbor Logistics',
    effectiveDate: '2026-01-01',
    expirationDate: '2026-12-31',
    effectiveEndDate: '2026-12-31',
    status: 'Active',
    coverageTypes: [],
  };

  it('emits the chosen policy and clears the search', async () => {
    const fixture = TestBed.createComponent(PolicyPicker);
    fixture.componentRef.setInput('results', [policy]);
    fixture.componentRef.setInput('isSearching', false);
    await fixture.whenStable();
    const chosen: Policy[] = [];
    const queries: string[] = [];
    fixture.componentInstance.chosen.subscribe((value) => chosen.push(value));
    fixture.componentInstance.query.subscribe((value) => queries.push(value));
    const picker = fixture.componentInstance as unknown as Record<string, (event: unknown) => void>;

    picker['choose'].call(fixture.componentInstance, { option: { value: policy } } as MatAutocompleteSelectedEvent);

    expect(chosen).toEqual([policy]);
    expect(queries).toEqual(['']);
  });
});
