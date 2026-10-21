import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import { ChangeStatusRequest, StatusCheck } from '../../core/api/api.model';
import { askStatusChange } from './status-change-dialog';

describe('askStatusChange (S:UI-Detail-Header, S:ST-Open-Closed)', () => {
  const error = (field: string, message: string) => ({ code: field, message, field, kind: 'Validation' as const });

  async function open(check: StatusCheck) {
    const answers: ChangeStatusRequest[] = [];
    askStatusChange(TestBed.inject(MatDialog), { from: 'Open', to: 'Closed', check }).subscribe((request) =>
      answers.push(request),
    );
    const dialogRef = TestBed.inject(MatDialog).openDialogs[0];
    await firstValueFrom(dialogRef.afterOpened());
    const container = () => document.querySelector('mat-dialog-container') as HTMLElement;
    const confirm = () =>
      [...container().querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim() === 'Confirm');

    return { answers, container, confirm, dialogRef };
  }

  afterEach(() => TestBed.inject(MatDialog).closeAll());

  it('lists the closure checklist and blocks the change while a check fails', async () => {
    const { container, confirm } = await open({
      errors: [error('Reserves', 'Settle the open reserves first.')],
      warnings: [{ code: 'W', message: 'No documents attached.', field: '' }],
    });

    expect(container().textContent).toContain('Closure checklist');
    expect(container().textContent).toContain('Settle the open reserves first.');
    expect(container().textContent).toContain('No documents attached.');
    expect(confirm()?.disabled).toBe(true);
  });

  it('asks for the note the check reports missing and sends it with the target status', async () => {
    const { answers, container, confirm, dialogRef } = await open({
      errors: [error('Note', 'Add a closing note.')],
      warnings: [],
    });
    const disabledBefore = confirm()?.disabled;
    const note = container().querySelector('textarea') as HTMLTextAreaElement;

    note.value = ' Settled with the insured. ';
    note.dispatchEvent(new Event('input'));
    await TestBed.inject(ApplicationRef).whenStable();
    confirm()?.click();
    await firstValueFrom(dialogRef.afterClosed());

    expect(disabledBefore).toBe(true);
    expect(answers).toEqual([
      expect.objectContaining({ targetStatus: 'Closed', note: 'Settled with the insured.', reasonCode: '' }),
    ]);
  });
});
