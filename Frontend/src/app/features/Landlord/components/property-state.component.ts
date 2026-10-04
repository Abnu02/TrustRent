import { Component, Input } from '@angular/core';
import { PropertyStatus } from '../services/property.service';

@Component({
  selector: 'app-property-state',
  standalone: true,
  template: `
    <span class="state-badge" [class.pending]="status === 'Pending'" [class.approved]="status === 'Approved'" [class.rejected]="status === 'Rejected'">{{ status }}</span>
    <span class="verification-badge" [class.verified]="isVerified">{{ isVerified ? 'Verified' : 'Not Verified' }}</span>
  `,
  styles: [`
    :host { display: inline-flex; align-items: center; flex-wrap: wrap; gap: 6px; }
    .state-badge, .verification-badge { display: inline-flex; align-items: center; min-height: 22px; padding: 3px 9px; border-radius: 999px; background: #e7ebf2; color: #394353; font-size: 10px; font-weight: 650; white-space: nowrap; }
    .state-badge.pending { background: #ffebd8; color: #7a3f11; }
    .state-badge.approved, .verification-badge.verified { background: #d8f7e9; color: #005b3f; }
    .state-badge.rejected, .verification-badge:not(.verified) { background: #ffdad6; color: #8f2020; }
  `],
})
export class PropertyStateComponent {
  @Input({ required: true }) status!: PropertyStatus;
  @Input({ required: true }) isVerified!: boolean;
}