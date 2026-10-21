import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, untracked } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterOutlet } from '@angular/router';
import { from, switchMap } from 'rxjs';
import { AuthService } from './core/auth/auth.service';
import { LoadingService } from './core/http/loading.service';
import { AppHeader } from './core/layout/app-header';
import { promptWith } from './shared/ui/prompt-dialog';

export const ACCESS_CODE_PROMPT = {
  title: 'Access code',
  message: 'This environment needs an access code for the test sign-in.',
  label: 'Access code',
  confirmLabel: 'Sign in',
  maxLength: 200,
};

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatProgressBarModule, AppHeader],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly loading = inject(LoadingService);
  private readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    effect(() => {
      if (this.auth.needsAccessCode()) {
        untracked(() => this.askAccessCode());
      }
    });
  }

  private askAccessCode(): void {
    promptWith(this.dialog, ACCESS_CODE_PROMPT)
      .pipe(
        switchMap((code) => from(this.auth.useAccessCode(code))),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({ next: () => this.document.location.reload(), error: () => undefined });
  }
}
