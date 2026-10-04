import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AdminIcon } from './components/admin-icon/admin-icon';
import { AdminSidebar } from './components/admin-sidebar/admin-sidebar';
import { AdminAuthSession } from './services/admin-auth-session';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, AdminIcon, AdminSidebar],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  sidebarCollapsed = false;
  readonly authSession: AdminAuthSession;

  constructor(
    private readonly router: Router,
    authSession: AdminAuthSession,
  ) {
    this.authSession = authSession;
  }

  signOut(): void {
    this.authSession.signOut();
    void this.router.navigateByUrl('/');
  }
}
