import { Component, EventEmitter, Input, Output } from '@angular/core';
import {
  RouterLink,
  RouterLinkActive
} from '@angular/router';

@Component({
  selector: 'app-landlord-sidebar',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './landlord-sidebar.component.html',
  styleUrl: './landlord-sidebar.component.scss'
})
export class LandlordSidebarComponent {
  @Input() collapsed = false;

  @Output() toggle = new EventEmitter<void>();

  onToggle(): void {
    this.toggle.emit();
  }}