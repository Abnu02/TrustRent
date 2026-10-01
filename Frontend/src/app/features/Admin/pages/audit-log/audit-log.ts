import { Component } from '@angular/core';
import { AdminIcon } from '../../components/admin-icon/admin-icon';

@Component({
  selector: 'app-admin-audit-log',
  standalone: true,
  imports: [AdminIcon],
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.scss',
})
export class AuditLog {
  readonly entries = [
    { action: 'Property approved', subject: '5402 Avenue F', actor: 'Admin', time: 'Today, 10:42 AM', reference: 'TR-2023-A5402', kind: 'approved' },
    { action: 'Landlord verification requested', subject: 'Elena Rostova', actor: 'Admin', time: 'Today, 09:18 AM', reference: 'LV-2024-0198', kind: 'pending' },
    { action: 'Documents submitted', subject: '1904 Guadalupe St #3', actor: 'Jonathan Miller', time: 'Yesterday, 04:32 PM', reference: 'TR-2024-B1904', kind: 'submitted' },
    { action: 'Property flagged', subject: '3300 Palm Way #402', actor: 'Automated review', time: 'Yesterday, 02:05 PM', reference: 'TR-2024-C3300', kind: 'flagged' },
    { action: 'Landlord verified', subject: 'Priya Shah', actor: 'Admin', time: 'Oct 28, 11:14 AM', reference: 'LV-2024-0186', kind: 'approved' },
  ];
}
