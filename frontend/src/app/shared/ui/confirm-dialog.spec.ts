import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { confirmWith } from './confirm-dialog';

describe('confirmWith (S:UI-Intake-Submit)', () => {
  it('shows the details and answers true when confirmed, false when closed otherwise', async () => {
    const dialog = TestBed.inject(MatDialog);
    const answers: boolean[] = [];

    confirmWith(dialog, {
      title: 'Create?',
      message: 'Check',
      details: ['No policy'],
      confirmLabel: 'Create',
    }).subscribe((answer) => answers.push(answer));
    await TestBed.inject(MatDialog).openDialogs[0].afterOpened().toPromise();
    const shown = document.querySelector('mat-dialog-container')?.textContent ?? '';
    document.querySelector<HTMLButtonElement>('mat-dialog-actions button:last-child')?.click();
    await dialog.openDialogs[0]?.afterClosed().toPromise();

    expect(shown).toContain('No policy');
    expect(answers).toEqual([true]);
  });
});
