import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotifyService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, 'Close', { panelClass: 'notify-success' });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Close', { panelClass: 'notify-error', duration: 8000 });
  }
}
