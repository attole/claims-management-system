import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { Policy } from '../../core/api/api.model';
import { PolicyPicker } from '../../shared/ui/policy-picker';
import { askPolicyToLink, LinkPolicyData, LinkPolicyDialog } from './link-policy-dialog';

describe('LinkPolicyDialog (S:API-PUT-claims-policy, A:API-GET-policies-coverage)', () => {
  const expired: Policy = {
    id: 'p2',
    policyNumber: 'POL-48112',
    clientName: 'Harbor Logistics',
    effectiveDate: '2025-01-01',
    expirationDate: '2025-12-31',
    effectiveEndDate: '2025-12-31',
    status: 'Expired',
    coverageTypes: ['Fire', 'Theft'],
  };

  async function render() {
    const close = vi.fn();
    const queries: string[] = [];
    const data: LinkPolicyData = {
      lossDate: '2026-07-22T09:40:00+00:00',
      results: signal([expired]),
      isSearching: signal(false),
      query: (text) => queries.push(text),
    };
    TestBed.configureTestingModule({
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: { close } },
      ],
    });
    const fixture = TestBed.createComponent(LinkPolicyDialog);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const link = () => [...element.querySelectorAll('button')].find((button) => button.textContent?.trim() === 'Link');

    return { fixture, element, close, queries, link };
  }

  it('answers nothing when the dialog is cancelled', async () => {
    const answers: Policy[] = [];
    const data: LinkPolicyData = {
      lossDate: '2026-07-22T09:40:00+00:00',
      results: signal([expired]),
      isSearching: signal(false),
      query: () => undefined,
    };
    const dialog = TestBed.inject(MatDialog);
    askPolicyToLink(dialog, data).subscribe((policy) => answers.push(policy));
    const dialogRef = dialog.openDialogs[0];
    await firstValueFrom(dialogRef.afterOpened());
    const cancel = [...document.querySelectorAll<HTMLButtonElement>('mat-dialog-container button')].find(
      (b) => b.textContent?.trim() === 'Cancel',
    );

    cancel?.click();
    await firstValueFrom(dialogRef.afterClosed());

    expect(answers).toEqual([]);
  });

  it('keeps Link off until a policy is chosen', async () => {
    const { link } = await render();

    expect(link()?.disabled).toBe(true);
  });

  it('warns about a policy outside the loss date, and links it on confirm', async () => {
    const { fixture, element, close, link } = await render();
    const picker = fixture.debugElement.query(By.directive(PolicyPicker)).componentInstance as PolicyPicker;

    picker.chosen.emit(expired);
    await fixture.whenStable();
    link()?.click();

    expect(element.textContent).toContain('The link is saved with a warning.');
    expect(element.querySelector('.picked')?.textContent).toContain('Covers Fire, Theft');
    expect(close).toHaveBeenCalledWith(expired);
  });

  it('passes the typed search to the store', async () => {
    const { fixture, queries } = await render();
    const picker = fixture.debugElement.query(By.directive(PolicyPicker)).componentInstance as PolicyPicker;

    picker.query.emit('harbor');

    expect(queries).toEqual(['harbor']);
  });
});
