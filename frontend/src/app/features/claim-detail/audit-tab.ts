import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { AuditEntry, PagedResult } from '../../core/api/api.model';
import { userName } from '../../core/auth/dev-users';
import { ClaimDatePipe } from '../../shared/pipes/claim-date.pipe';
import { EnumLabelPipe } from '../../shared/pipes/enum-label.pipe';
import { AUDIT_PAGE_SIZES, AuditPaging } from './claim-detail.store';

@Component({
  selector: 'app-audit-tab',
  imports: [
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTableModule,
    ClaimDatePipe,
    EnumLabelPipe,
  ],
  templateUrl: './audit-tab.html',
  styleUrl: './audit-tab.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditTab {
  readonly page = input.required<PagedResult<AuditEntry>>();
  readonly isLoading = input(false);
  readonly hasError = input(false);

  readonly pageChanged = output<AuditPaging>();
  readonly retried = output();

  protected readonly pageSizes = AUDIT_PAGE_SIZES;
  protected readonly columns = ['createdAt', 'eventType', 'description', 'user', 'related'];
  protected readonly rows = computed(() =>
    this.page().items.map((entry) => ({ ...entry, userName: userName(entry.createdByUserId) })),
  );

  protected changePage(event: PageEvent): void {
    this.pageChanged.emit({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }
}
