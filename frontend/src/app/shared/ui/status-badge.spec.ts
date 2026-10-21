import { TestBed } from '@angular/core/testing';
import { StatusBadge } from './status-badge';

describe('StatusBadge (S:UI-Badges)', () => {
  it.each([
    ['Draft', 'badge--draft', 'Draft'],
    ['UnderInvestigation', 'badge--under-investigation', 'Under Investigation'],
    ['PendingPayment', 'badge--pending-payment', 'Pending Payment'],
    ['Withdrawn', 'badge--withdrawn', 'Withdrawn'],
  ])('%s gets its tone class and a readable label', async (status, tone, label) => {
    const fixture = TestBed.createComponent(StatusBadge);
    fixture.componentRef.setInput('status', status);

    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.classList).toContain('badge');
    expect(element.classList).toContain(tone);
    expect(element.textContent?.trim()).toBe(label);
  });
});
