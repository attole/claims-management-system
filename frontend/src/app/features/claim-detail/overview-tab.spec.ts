import { TestBed } from '@angular/core/testing';
import { Issue } from '../../core/api/api.model';
import { claimDetail, RAISED_ISSUE } from './claim-detail.fixtures';
import { OverviewTab } from './overview-tab';

describe('OverviewTab (S:UI-Detail-Tabs)', () => {
  async function render() {
    const fixture = TestBed.createComponent(OverviewTab);
    fixture.componentRef.setInput('detail', claimDetail());
    fixture.componentRef.setInput('causeName', 'Water damage');
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const button = (text: string) =>
      [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim() === text);

    return { fixture, element, button };
  }

  it('shows the loss event with its estimate and severity', async () => {
    const { element } = await render();
    const text = element.textContent ?? '';

    expect(text).toContain('Pipe burst under the kitchen floor.');
    expect(text).toContain('$62,500.00');
    expect(text).toContain('Standard');
  });

  it('saves edited notes, and keeps the edit when the claim reloads', async () => {
    const { fixture, element, button } = await render();
    const saved: string[] = [];
    fixture.componentInstance.notesSaved.subscribe((notes) => saved.push(notes));
    const notes = element.querySelector('textarea') as HTMLTextAreaElement;
    const initial = notes.value;

    notes.value = ' Report received. ';
    notes.dispatchEvent(new Event('input'));
    fixture.componentRef.setInput('detail', claimDetail());
    await fixture.whenStable();
    button('Save notes')?.click();

    expect(initial).toBe('Plumber report requested.');
    expect(notes.value).toBe(' Report received. ');
    expect(saved).toEqual(['Report received.']);
  });

  it('keeps unsaved notes after a refused save, and settles once the reload carries them', async () => {
    const { fixture, element, button } = await render();
    const notes = element.querySelector('textarea') as HTMLTextAreaElement;
    notes.value = 'Report received.';
    notes.dispatchEvent(new Event('input'));
    button('Save notes')?.click();
    fixture.componentRef.setInput('detail', claimDetail());
    await fixture.whenStable();
    const afterRefusal = [notes.value, button('Save notes')?.disabled];
    const detail = claimDetail();

    fixture.componentRef.setInput('detail', { ...detail, claim: { ...detail.claim, notes: 'Report received.' } });
    await fixture.whenStable();

    expect(afterRefusal).toEqual(['Report received.', false]);
    expect(button('Save notes')?.disabled).toBe(true);
  });

  it('acknowledges or waives an open issue', async () => {
    const { fixture, button } = await render();
    const actions: [string, Issue][] = [];
    fixture.componentInstance.acknowledged.subscribe((issue) => actions.push(['acknowledge', issue]));
    fixture.componentInstance.waived.subscribe((issue) => actions.push(['waive', issue]));

    button('Acknowledge')?.click();
    button('Waive')?.click();

    expect(actions).toEqual([
      ['acknowledge', RAISED_ISSUE],
      ['waive', RAISED_ISSUE],
    ]);
  });
});
