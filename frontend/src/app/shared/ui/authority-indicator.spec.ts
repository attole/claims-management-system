import { TestBed } from '@angular/core/testing';
import { AuthorityIndicator } from './authority-indicator';

describe('AuthorityIndicator (S:UI-Intake-Reserve, S:UI-Reserves)', () => {
  it.each([
    [0, 'Enter an amount', 'indicator--none'],
    [5_000, 'Auto-approved (up to $10,000.00)', 'indicator--auto'],
    [60_000, 'Supervisor approval required', 'indicator--supervisor'],
    [250_000, 'Manager approval required', 'indicator--manager'],
  ])('for %s says "%s"', async (amount, text, tone) => {
    const fixture = TestBed.createComponent(AuthorityIndicator);
    fixture.componentRef.setInput('amount', amount);

    await fixture.whenStable();

    const element = (fixture.nativeElement as HTMLElement).querySelector('.indicator');
    expect(element?.textContent).toContain(text);
    expect(element?.classList).toContain(tone);
  });
});
