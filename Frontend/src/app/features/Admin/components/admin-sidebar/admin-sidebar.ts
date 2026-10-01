import { Component, HostBinding, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AdminIcon, AdminIconName } from '../admin-icon/admin-icon';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [AdminIcon, RouterLink, RouterLinkActive],
  templateUrl: './admin-sidebar.html',
  styleUrl: './admin-sidebar.scss',
})
export class AdminSidebar {
  readonly collapsedChange = output<boolean>();
  collapsed = false;
  @HostBinding('class.is-collapsed')
  get isCollapsed(): boolean {
    return this.collapsed;
  }
  readonly navigation: ReadonlyArray<{ label: string; route: string; icon: AdminIconName }> = [
    { label: 'Overview', route: '/admin', icon: 'overview' },
    { label: 'Properties', route: '/admin/properties', icon: 'property' },
    { label: 'Landlord reviews', route: '/admin/landlords', icon: 'landlords' },
    { label: 'Audit log', route: '/admin/audit-log', icon: 'audit' },
  ];

  toggle(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }
}
