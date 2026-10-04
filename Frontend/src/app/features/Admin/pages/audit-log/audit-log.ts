import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { AdminIcon } from '../../components/admin-icon/admin-icon';
import { AdminAuditApi, AuditLogEntry } from '../../services/admin-audit-api';

@Component({
  selector: 'app-admin-audit-log',
  standalone: true,
  imports: [AdminIcon, DatePipe],
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.scss',
})
export class AuditLog implements OnInit {
  private readonly api = inject(AdminAuditApi);
  entries: AuditLogEntry[] = [];
  loading = true;
  errorMessage = '';

  async ngOnInit(): Promise<void> {
    try {
      this.entries = await this.api.getRecent();
    } catch (error) {
      this.errorMessage = error instanceof HttpErrorResponse && error.status === 0
        ? 'Could not connect to the TrustRent API. Check that the backend is running.'
        : error instanceof HttpErrorResponse && error.status === 401
          ? 'Your session expired. Sign in again.'
          : error instanceof HttpErrorResponse && error.status === 403
            ? 'Admin access is required to view the audit log.'
            : 'Audit history could not be loaded.';
    } finally {
      this.loading = false;
    }
  }
}
