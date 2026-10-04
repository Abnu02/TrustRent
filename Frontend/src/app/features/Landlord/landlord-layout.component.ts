import { Component, DestroyRef, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { FeedbackService } from './services/feedback.service';
import { LandlordIconComponent } from './components/landlord-icon.component';

@Component({
  selector: 'app-landlord-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, LandlordIconComponent],
  templateUrl: './landlord-layout.component.html',
  styleUrl: './landlord-layout.component.scss',
})
export class LandlordLayoutComponent {
  private readonly feedback = inject(FeedbackService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  readonly successMessage = this.feedback.successMessage;
  readonly user = this.auth.user;

  dismissSuccess(): void {
    this.feedback.dismiss();
  }

  signOut(): void {
    this.auth.logout().pipe(
      catchError(() => of({ message: 'Signed out locally.' })),
      finalize(() => {
        this.auth.clearSession();
        void this.router.navigate(['/auth']);
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe();
  }
}