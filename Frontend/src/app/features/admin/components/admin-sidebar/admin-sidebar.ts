import { Component, EventEmitter, Input, Output } from '@angular/core';
import {
  RouterLink,
  RouterLinkActive
} from '@angular/router';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './admin-sidebar.html',
  styleUrl: './admin-sidebar.scss'
})
export class AdminSidebarComponent {

  @Input() collapsed = false;

  @Output() toggle = new EventEmitter<void>();

  onToggle(): void {
    this.toggle.emit();
  }
}