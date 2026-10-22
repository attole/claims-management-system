import { TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { CauseField } from './cause-field';

describe('CauseField (S:UI-Intake-Steps)', () => {
  const causes = [
    { code: 'COL-WATER', name: 'Water damage', perilCategory: 'Property' },
    { code: 'COL-FIRE', name: 'Fire', perilCategory: 'Property' },
  ];

  async function render(code = '') {
    const control = new FormControl(code, { nonNullable: true });
    const fixture = TestBed.createComponent(CauseField);
    fixture.componentRef.setInput('control', control);
    fixture.componentRef.setInput('causeCodes', causes);
    await fixture.whenStable();
    const field = fixture.componentInstance as unknown as Record<string, unknown>;

    return { fixture, control, field, search: field['causeSearch'] as FormControl<string> };
  }

  it('shows the name of a saved cause code', async () => {
    const { search } = await render('COL-FIRE');

    expect(search.value).toBe('Fire');
  });

  it('a cause typed by its name resolves to its code', async () => {
    const { fixture, control, field, search } = await render();

    search.setValue('water damage');
    (field['typeCause'] as () => void).call(fixture.componentInstance);

    expect(control.value).toBe('COL-WATER');
    expect(control.touched).toBe(true);
  });

  it('keeps an unknown code so the validator can flag it', async () => {
    const { search } = await render('COL-RETIRED');

    expect(search.value).toBe('COL-RETIRED');
  });
});
