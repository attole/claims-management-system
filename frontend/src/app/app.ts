import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterOutlet } from '@angular/router';
import { LoadingService } from './core/http/loading.service';
import { AppHeader } from './core/layout/app-header';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatProgressBarModule, AppHeader],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly loading = inject(LoadingService);
}
