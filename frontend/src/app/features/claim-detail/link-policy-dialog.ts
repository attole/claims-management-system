import { ChangeDetectionStrategy, Component, computed, inject, Signal, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { filter, Observable } from 'rxjs';
import { isSetDate, parseIsoDateTime } from '../../core/api/api-dates';
import { Policy } from '../../core/api/api.model';
import { PolicyPicker } from '../../shared/ui/policy-picker';
import { inForce } from '../../shared/util/in-force';

export interface LinkPolicyData {
  lossDate: string;
  results: Signal<Policy[]>;
  isSearching: Signal<boolean>;
  query: (text: string) => void;
}

@Component({
  selector: 'app-link-policy-dialog',
  imports: [MatButtonModule, MatDialogModule, PolicyPicker],
  templateUrl: './link-policy-dialog.html',
  styleUrl: './link-policy-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LinkPolicyDialog {
  protected readonly data = inject<LinkPolicyData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<LinkPolicyDialog, Policy>>(MatDialogRef);

  protected readonly chosen = signal<Policy | null>(null);
  protected readonly inForce = computed(() => {
    const lossDate = isSetDate(this.data.lossDate) ? parseIsoDateTime(this.data.lossDate) : null;

    return inForce(this.chosen(), lossDate);
  });

  protected link(): void {
    const policy = this.chosen();

    if (policy) {
      this.dialogRef.close(policy);
    }
  }
}

export function askPolicyToLink(dialog: MatDialog, data: LinkPolicyData): Observable<Policy> {
  return dialog
    .open<LinkPolicyDialog, LinkPolicyData, Policy>(LinkPolicyDialog, { data, width: '560px' })
    .afterClosed()
    .pipe(filter((policy): policy is Policy => typeof policy === 'object'));
}
