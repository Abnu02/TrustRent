import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { AdminNavbarComponent } from '../../components/admin-navbar/admin-navbar';
import { AdminSidebarComponent } from '../../components/admin-sidebar/admin-sidebar';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    AdminNavbarComponent,
    AdminSidebarComponent
  ],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayoutComponent {

  sidebarOpen = signal(true);

  toggleSidebar(): void {
    this.sidebarOpen.update(open => !open);
  }
}