import { ChangeDetectionStrategy, Component, computed, effect, input, model, output, untracked } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import {
  CauseCode,
  ClaimDetail,
  ClaimSeverity,
  Issue,
  IssueSeverity,
  LossRequest,
  RiskObject,
  UserRole,
} from '../../core/api/api.model';
import { RiskObjectValue } from '../../shared/forms/risk-object-form';
import { ClaimDatePipe } from '../../shared/pipes/claim-date.pipe';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { MoneyPipe } from '../../shared/pipes/money.pipe';
import { RiskObjectEditor } from '../../shared/ui/risk-object-editor';
import { canAcknowledge, canWaive } from '../../shared/util/action-gates';
import { LossEditor } from './loss-editor';

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
    LossEditor,
    MoneyPipe,
    RiskObjectEditor,
  ],
  templateUrl: './overview-tab.html',
  styleUrl: './overview-tab.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OverviewTab {
  readonly detail = input.required<ClaimDetail>();
  readonly causeName = input.required<string>();
  readonly causeCodes = input<CauseCode[]>([]);
  readonly role = input<UserRole>('Handler');
  readonly isSaving = input(false);
  readonly isEditable = input(true);
  readonly isEditingLoss = model(false);
  readonly isAddingRiskObject = model(false);

  readonly notesSaved = output<string>();
  readonly acknowledged = output<Issue>();
  readonly waived = output<Issue>();
  readonly lossSaved = output<LossRequest>();
  readonly riskObjectAdded = output<RiskObjectValue>();
  readonly primaryToggled = output<RiskObject>();
  readonly riskObjectRemoved = output<RiskObject>();

  protected readonly notes = new FormControl('', { nonNullable: true });
  protected readonly severityTone = computed(() => SEVERITY_TONES[this.detail().claim.severity]);
  protected readonly openIssues = computed(() =>
    this.detail()
      .issues.filter((issue) => issue.state === 'Raised')
      .map((issue) => ({ issue, canAcknowledge: canAcknowledge(issue), canWaive: canWaive(issue, this.role()) })),
  );
  protected readonly resolvedCount = computed(() => this.detail().issues.length - this.openIssues().length);
  protected readonly riskObjects = computed(() => this.detail().riskObjects.filter((item) => !item.isDeleted));
  protected readonly hasPrimary = computed(() => this.riskObjects().some((item) => item.isPrimary));
  protected readonly primaryName = computed(
    () => this.riskObjects().find((item) => item.isPrimary)?.assetDescription ?? '',
  );
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

  protected canMakePrimary(item: RiskObject): boolean {
    return item.isPrimary || !this.hasPrimary();
  }
}
