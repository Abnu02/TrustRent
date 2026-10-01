import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { FeedbackService } from '../feedback.service';

@Component({
  selector: 'app-landlord-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './landlord-layout.component.html',
  styleUrl: './landlord-layout.component.scss',
})
export class LandlordLayoutComponent {
  private readonly feedback = inject(FeedbackService);
  readonly successMessage = this.feedback.successMessage;

  dismissSuccess(): void {
    this.feedback.dismiss();
  }
}