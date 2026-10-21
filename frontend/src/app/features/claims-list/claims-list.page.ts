import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { Router, RouterLink } from '@angular/router';
import { ClaimSummary } from '../../core/api/api.model';
import { DEV_USERS } from '../../core/auth/dev-users';
import { ClaimDatePipe } from '../../shared/pipes/claim-date.pipe';
import { MoneyPipe } from '../../shared/pipes/money.pipe';
import { StatusBadge } from '../../shared/ui/status-badge';
import { ClaimsFilterBar } from './claims-filter-bar';
import { ClaimsListStore, PAGE_SIZES } from './claims-list.store';

@Component({
  selector: 'app-claims-list-page',
  imports: [
    RouterLink,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTableModule,
    ClaimDatePipe,
    MoneyPipe,
    StatusBadge,
    ClaimsFilterBar,
  ],
  templateUrl: './claims-list.page.html',
  styleUrl: './claims-list.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ClaimsListStore],
})
export class ClaimsListPage {
  protected readonly store = inject(ClaimsListStore);
  private readonly router = inject(Router);

  protected readonly handlers = DEV_USERS;
  protected readonly pageSizes = PAGE_SIZES;
  protected readonly columns = [
    'claimNumber',
    'policyNumber',
    'clientName',
    'lossDate',
    'causeOfLoss',
    'status',
    'totalReserves',
  ];

  protected open(claim: ClaimSummary): void {
    void this.router.navigate(['/claims', claim.id]);
  }
}
