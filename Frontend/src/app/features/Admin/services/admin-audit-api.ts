import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export interface AuditLogEntry {
  id: string;
  propertyId: string | null;
  action: string;
  subject: string;
  actorName: string;
  actorEmail: string;
  status: string;
  reference: string;
  note: string | null;
  occurredAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminAuditApi {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/v1/admin/audit-log';

  getRecent(): Promise<AuditLogEntry[]> {
    return firstValueFrom(this.http.get<AuditLogEntry[]>(this.endpoint));
  }
}
