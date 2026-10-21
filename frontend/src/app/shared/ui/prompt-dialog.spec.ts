import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import { promptWith } from './prompt-dialog';

describe('promptWith (S:API-POST-reserve-reject)', () => {
  const data = { title: 'Reject?', message: 'Why?', label: 'Reason', confirmLabel: 'Reject', maxLength: 500 };

  async function open() {
    const answers: string[] = [];
    promptWith(TestBed.inject(MatDialog), data).subscribe((answer) => answers.push(answer));
    const dialogRef = TestBed.inject(MatDialog).openDialogs[0];
    await firstValueFrom(dialogRef.afterOpened());
    const container = document.querySelector('mat-dialog-container') as HTMLElement;
    const type = async (text: string) => {
      const field = container.querySelector('textarea') as HTMLTextAreaElement;
      field.value = text;
      field.dispatchEvent(new Event('input'));
      container.querySelector<HTMLButtonElement>('button[type="submit"]')?.click();
      await TestBed.inject(ApplicationRef).whenStable();
    };

    return { answers, dialogRef, container, type };
  }

  afterEach(() => TestBed.inject(MatDialog).closeAll());

  it('answers the trimmed text', async () => {
    const { answers, dialogRef, type } = await open();

    await type('  Amount not supported by the report.  ');
    await firstValueFrom(dialogRef.afterClosed());

    expect(answers).toEqual(['Amount not supported by the report.']);
  });

  it('stays open and says why when the answer is blank', async () => {
    const { answers, container, type } = await open();

    await type('   ');

    expect(answers).toEqual([]);
    expect(container.textContent).toContain('This field is required.');
  });
});
