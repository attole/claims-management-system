import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-claims-list-page',
  templateUrl: './claims-list.page.html',
  styleUrl: './claims-list.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimsListPage {}
