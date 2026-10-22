import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Issue, LossRequest, RiskObject } from '../../core/api/api.model';
import { provideAppDateAdapter } from '../../core/dates/app-date-adapter';
import { claimDetail, OTHER_RISK_OBJECT, PRIMARY_RISK_OBJECT, RAISED_ISSUE } from './claim-detail.fixtures';
import { LossEditor } from './loss-editor';
import { OverviewTab } from './overview-tab';

describe('OverviewTab (S:UI-Detail-Tabs)', () => {
  async function render() {
    TestBed.configureTestingModule({ providers: [...provideAppDateAdapter()] });
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

  it('offers Acknowledge on a warning and Waive on a critical issue to a supervisor only', async () => {
    const { fixture, element, button } = await render();
    const critical = { ...RAISED_ISSUE, id: 'i2', severity: 'Critical' as const, code: 'S:BR-ST-02' };
    const actions: [string, Issue][] = [];
    fixture.componentInstance.acknowledged.subscribe((issue) => actions.push(['acknowledge', issue]));
    fixture.componentInstance.waived.subscribe((issue) => actions.push(['waive', issue]));
    fixture.componentRef.setInput('detail', { ...claimDetail(), issues: [RAISED_ISSUE, critical] });
    await fixture.whenStable();
    const waiveForHandler = button('Waive');
    fixture.componentRef.setInput('role', 'Supervisor');
    await fixture.whenStable();

    button('Acknowledge')?.click();
    button('Waive')?.click();

    expect(waiveForHandler).toBeUndefined();
    expect(element.querySelectorAll('[aria-labelledby="issues-title"] li button').length).toBe(2);
    expect(actions).toEqual([
      ['acknowledge', RAISED_ISSUE],
      ['waive', critical],
    ]);
  });

  it('Edit opens the loss editor, and a save emits the loss request', async () => {
    const { fixture, element, button } = await render();
    const saved: LossRequest[] = [];
    fixture.componentInstance.lossSaved.subscribe((loss) => saved.push(loss));

    button('Edit')?.click();
    await fixture.whenStable();
    const editor = fixture.debugElement.query(By.directive(LossEditor)).componentInstance as LossEditor;
    const request = { ...claimDetail().loss, estimatedLossAmount: 1 };
    editor.submitted.emit(request);

    expect(element.querySelector('app-loss-editor')).not.toBeNull();
    expect(saved).toEqual([request]);
  });

  it('lets the primary be unset, and blocks a second primary until then', async () => {
    const { fixture, element } = await render();
    const toggled: RiskObject[] = [];
    fixture.componentInstance.primaryToggled.subscribe((item) => toggled.push(item));
    const rows = [...element.querySelectorAll('[aria-labelledby="risk-title"] li')];
    const rowButton = (row: Element, text: string) =>
      [...row.querySelectorAll('button')].find((candidate) => candidate.textContent?.trim() === text);

    rowButton(rows[0], 'Unset primary')?.click();

    expect(rowButton(rows[1], 'Make primary')?.disabled).toBe(true);
    expect(toggled).toEqual([PRIMARY_RISK_OBJECT]);
  });

  it('removes a risk object', async () => {
    const { fixture, element } = await render();
    const removed: RiskObject[] = [];
    fixture.componentInstance.riskObjectRemoved.subscribe((item) => removed.push(item));
    const rows = [...element.querySelectorAll('[aria-labelledby="risk-title"] li')];

    [...rows[1].querySelectorAll('button')].find((candidate) => candidate.textContent?.trim() === 'Remove')?.click();

    expect(removed).toEqual([OTHER_RISK_OBJECT]);
  });

  it('keeps Make primary off when the primary has no description', async () => {
    const { fixture, element } = await render();
    const detail = claimDetail();
    fixture.componentRef.setInput('detail', {
      ...detail,
      riskObjects: [{ ...PRIMARY_RISK_OBJECT, assetDescription: '' }, OTHER_RISK_OBJECT],
    });
    await fixture.whenStable();
    const rows = [...element.querySelectorAll('[aria-labelledby="risk-title"] li')];

    const makePrimary = [...rows[1].querySelectorAll('button')].find(
      (candidate) => candidate.textContent?.trim() === 'Make primary',
    );

    expect(makePrimary?.disabled).toBe(true);
  });
});
