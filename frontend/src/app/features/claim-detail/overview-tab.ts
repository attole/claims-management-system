import { ChangeDetectionStrategy, Component, computed, effect, input, output, untracked } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ClaimDetail, ClaimSeverity, Issue, IssueSeverity } from '../../core/api/api.model';
import { ClaimDatePipe } from '../../shared/pipes/claim-date.pipe';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

const SEVERITY_TONES: Readonly<Record<ClaimSeverity, string>> = {
  Unspecified: '',
  Minor: 'badge--ok',
  Standard: 'badge--info',
  Critical: 'badge--warn',
  Catastrophic: 'badge--bad',
};

const ISSUE_TONES: Readonly<Record<IssueSeverity, string>> = {
  Critical: 'badge--bad',
  Warning: 'badge--warn',
};

@Component({
  selector: 'app-overview-tab',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    ClaimDatePipe,
    EnumLabelPipe,
    MoneyPipe,
  ],
  templateUrl: './overview-tab.html',
  styleUrl: './overview-tab.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OverviewTab {
  readonly detail = input.required<ClaimDetail>();
  readonly causeName = input.required<string>();
  readonly isSaving = input(false);

  readonly notesSaved = output<string>();
  readonly acknowledged = output<Issue>();
  readonly waived = output<Issue>();

  protected readonly notes = new FormControl('', { nonNullable: true });
  protected readonly severityTone = computed(() => SEVERITY_TONES[this.detail().claim.severity]);
  protected readonly openIssues = computed(() => this.detail().issues.filter((issue) => issue.state === 'Raised'));
  protected readonly resolvedCount = computed(() => this.detail().issues.length - this.openIssues().length);
  protected readonly riskObjects = computed(() => this.detail().riskObjects.filter((item) => !item.isDeleted));
  protected readonly issueTones = ISSUE_TONES;

  constructor() {
    effect(() => {
      const saved = this.detail().claim.notes;

      untracked(() => {
        const isSavedText = this.notes.value.trim() === saved;

        if (!this.notes.dirty || isSavedText) {
          this.notes.setValue(saved);
          this.notes.markAsPristine();
        }
      });
    });
  }

  protected saveNotes(): void {
    this.notesSaved.emit(this.notes.value.trim());
  }
}
