import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterLink } from '@angular/router';
import { filter, Observable, switchMap } from 'rxjs';
import { ClaimStatus, Issue, Party, Warning, WriteResult } from '../../core/api/api.model';
import { isRefusal, readErrorBody } from '../../core/http/error-body';
import { NotifyService } from '../../core/notify/notify.service';
import { partyName, PartyValue } from '../../shared/forms/party-form';
import { enumLabel } from '../../shared/pipes/enum-label.pipe';
import { confirmWith } from '../../shared/ui/confirm-dialog';
import { AuditTab } from './audit-tab';
import { ClaimDetailStore } from './claim-detail.store';
import { ClaimHeader } from './claim-header';
import { OverviewTab } from './overview-tab';
import { PartiesTab } from './parties-tab';
import { askStatusChange } from './status-change-dialog';

@Component({
  selector: 'app-claim-detail-page',
  imports: [
    RouterLink,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    ClaimHeader,
    OverviewTab,
    PartiesTab,
    AuditTab,
  ],
  templateUrl: './claim-detail.page.html',
  styleUrl: './claim-detail.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ClaimDetailStore],
})
export class ClaimDetailPage {
  readonly claimId = input.required<string>();

  protected readonly store = inject(ClaimDetailStore);
  private readonly dialog = inject(MatDialog);
  private readonly notify = inject(NotifyService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly bannerMessages = signal<string[]>([]);
  protected readonly isAddingParty = signal(false);

  constructor() {
    effect(() => {
      const claimId = this.claimId();
      untracked(() => this.store.claimId.set(claimId));
    });
  }

  protected changeStatus(to: ClaimStatus): void {
    const from = this.store.status();
    const change = this.store.checkStatus(to).pipe(
      switchMap((check) => askStatusChange(this.dialog, { from, to, check })),
      switchMap((request) => this.store.changeStatus(request)),
    );

    this.run(change, `Status changed to ${enumLabel(to)}`);
  }

  protected saveNotes(notes: string): void {
    this.run(this.store.saveNotes(notes), 'Notes saved');
  }

  protected addParty(party: PartyValue): void {
    this.run(this.store.addParty(party), 'Party added', () => this.isAddingParty.set(false));
  }

  protected removeParty(party: Party): void {
    const confirmation = {
      title: 'Remove party?',
      message: `${partyName(party)} stays in the history but is no longer active on the claim.`,
      details: [],
      confirmLabel: 'Remove',
    };
    const removal = confirmWith(this.dialog, confirmation).pipe(
      filter((isConfirmed) => isConfirmed),
      switchMap(() => this.store.removeParty(party)),
    );

    this.run(removal, 'Party removed');
  }

  protected acknowledgeIssue(issue: Issue): void {
    this.run(this.store.acknowledgeIssue(issue), 'Issue acknowledged');
  }

  protected waiveIssue(issue: Issue): void {
    this.run(this.store.waiveIssue(issue), 'Issue waived');
  }

  protected copied(isCopied: boolean): void {
    if (isCopied) {
      this.notify.success('Claim number copied.');
    }
  }

  private run<TData>(write: Observable<WriteResult<TData>>, message: string, then = (): void => undefined): void {
    write.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.bannerMessages.set([]);
        this.notify.success(`${message}${warningNote(result.warnings)}.`);
        then();
      },
      error: (error: unknown) => this.refused(error),
    });
  }

  private refused(error: unknown): void {
    if (isRefusal(error)) {
      const body = readErrorBody(error);
      const messages = Object.values(body.errors).flat();
      this.bannerMessages.set(messages.length > 0 ? messages : [body.title]);
    }
  }
}

function warningNote(warnings: Warning[]): string {
  return warnings.length > 0 ? ` with ${warnings.length} warning(s)` : '';
}
