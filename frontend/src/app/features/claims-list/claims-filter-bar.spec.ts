import { TestBed } from '@angular/core/testing';
import { provideAppDateAdapter } from '../../core/dates/app-date-adapter';
import { DEV_USERS } from '../../core/auth/dev-users';
import { ClaimsFilterBar, FILTER_DEBOUNCE_MS } from './claims-filter-bar';
import { emptyFilter, FilterChange } from './claims-list.store';

describe('ClaimsFilterBar (S:UI-List)', () => {
  async function render() {
    TestBed.configureTestingModule({ providers: [...provideAppDateAdapter()] });
    const fixture = TestBed.createComponent(ClaimsFilterBar);
    fixture.componentRef.setInput('filter', { ...emptyFilter(), search: 'start' });
    fixture.componentRef.setInput('causeCodes', [{ code: 'COL-FIRE', name: 'Fire', perilCategory: 'Property' }]);
    fixture.componentRef.setInput('handlers', DEV_USERS);
    await fixture.whenStable();
    const changes: FilterChange[] = [];
    fixture.componentInstance.changed.subscribe((change) => changes.push(change));

    return { fixture, changes, form: fixture.componentInstance['form'] };
  }

  afterEach(() => vi.useRealTimers());

  it('fills the form from the filter it is given', async () => {
    const { form } = await render();

    expect(form.controls.search.value).toBe('start');
  });

  it('emits one change after the debounce, with calendar dates and a trimmed search', async () => {
    const { form, changes } = await render();
    vi.useFakeTimers();
    form.controls.search.setValue('  CLM ');
    form.controls.dateFrom.setValue(new Date(2026, 0, 5));

    vi.advanceTimersByTime(FILTER_DEBOUNCE_MS);

    expect(changes.length).toBe(1);
    expect(changes[0]).toEqual(expect.objectContaining({ search: 'CLM', dateFrom: '2026-01-05', dateTo: '' }));
  });

  it('does not send a search longer than the API accepts', async () => {
    const { form, changes } = await render();
    vi.useFakeTimers();

    form.controls.search.setValue('x'.repeat(256));
    vi.advanceTimersByTime(FILTER_DEBOUNCE_MS);

    expect(form.controls.search.invalid).toBe(true);
    expect(changes).toEqual([]);
  });

  it('after a clear, choosing the same status again emits again', async () => {
    const { fixture, form, changes } = await render();
    vi.useFakeTimers();
    form.controls.statuses.setValue(['Open']);
    vi.advanceTimersByTime(FILTER_DEBOUNCE_MS);
    fixture.componentRef.setInput('filter', { ...emptyFilter(), statuses: ['Open'] });
    TestBed.tick();
    fixture.componentRef.setInput('filter', emptyFilter());
    TestBed.tick();

    form.controls.statuses.setValue(['Open']);
    vi.advanceTimersByTime(FILTER_DEBOUNCE_MS);

    expect(changes.length).toBe(2);
    expect(changes[1].statuses).toEqual(['Open']);
  });

  it('keeps the text being typed when the filter comes back trimmed', async () => {
    const { fixture, form, changes } = await render();
    vi.useFakeTimers();
    form.controls.search.setValue('Harbor ');
    vi.advanceTimersByTime(FILTER_DEBOUNCE_MS);

    fixture.componentRef.setInput('filter', { ...emptyFilter(), search: changes[0].search });
    TestBed.tick();

    expect(form.controls.search.value).toBe('Harbor ');
  });

  it('emits nothing while the end date is before the start date', async () => {
    const { form, changes } = await render();
    vi.useFakeTimers();
    form.controls.dateFrom.setValue(new Date(2026, 5, 10));
    form.controls.dateTo.setValue(new Date(2026, 5, 1));

    vi.advanceTimersByTime(FILTER_DEBOUNCE_MS);

    expect(changes).toEqual([]);
  });
});
