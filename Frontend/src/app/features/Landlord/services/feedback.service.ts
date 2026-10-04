import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  readonly successMessage = signal('');
  private clearTimer?: ReturnType<typeof setTimeout>;

  success(message: string): void {
    if (this.clearTimer) clearTimeout(this.clearTimer);
    this.successMessage.set(message);
    this.clearTimer = setTimeout(() => this.dismiss(), 5000);
  }

  dismiss(): void {
    this.successMessage.set('');
    if (this.clearTimer) clearTimeout(this.clearTimer);
  }
}