import { Component, Input } from '@angular/core';

export type AuthIconName =
  | 'admin_panel_settings'
  | 'arrow_forward'
  | 'badge'
  | 'check_circle'
  | 'dashboard'
  | 'error'
  | 'fact_check'
  | 'home_work'
  | 'lock'
  | 'lock_reset'
  | 'login'
  | 'mail'
  | 'person'
  | 'person_add'
  | 'phone'
  | 'progress_activity'
  | 'rule'
  | 'shield'
  | 'support_agent'
  | 'verified'
  | 'verified_user'
  | 'visibility'
  | 'visibility_off';

const iconPaths: Record<AuthIconName, readonly string[]> = {
  admin_panel_settings: [
    'M12 3 20 6v5c0 5-3.4 8.6-8 10-4.6-1.4-8-5-8-10V6l8-3Z',
    'M9 12a2 2 0 1 0 4 0 2 2 0 0 0-4 0Z',
    'M8 17c.7-1.3 1.9-2 3-2',
  ],
  arrow_forward: ['M5 12h14', 'm12 5 7 7-7 7'],
  badge: ['M12 3 15 5l4-.2.2 4L21 12l-2 3 .2 4-4-.2L12 21l-3-2-4 .2.2-4L3 12l2-3-.2-4L9 5l3-2Z', 'm9 12 2 2 4-4'],
  check_circle: ['M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20Z', 'm8 12 2.5 2.5L16 9'],
  dashboard: ['M3 3h8v8H3z', 'M13 3h8v5h-8z', 'M13 10h8v11h-8z', 'M3 13h8v8H3z'],
  error: ['M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20Z', 'M12 8v5', 'M12 16.5h.01'],
  fact_check: ['M9 5h10v16H5V5h2', 'M9 3h6v4H9z', 'm8 12 1.5 1.5L12 11', 'M14 12h3', 'm8 17 1.5 1.5L12 16', 'M14 17h3'],
  home_work: ['m3 10 9-7 9 7', 'M5 9v12h14V9', 'M9 21v-7h6v7', 'M15 6h4V3h-4'],
  lock: ['M5 10h14v11H5z', 'M8 10V7a4 4 0 0 1 8 0v3', 'M12 14v3'],
  lock_reset: ['M5 10h14v11H5z', 'M8 10V7a4 4 0 0 1 7.5-2', 'M12 14v3', 'M4 5v4h4'],
  login: ['M10 17l5-5-5-5', 'M15 12H3', 'M12 3h7a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-7'],
  mail: ['M3 5h18v14H3z', 'm3 7 9 6 9-6'],
  person: ['M20 21a8 8 0 0 0-16 0', 'M12 13a5 5 0 1 0 0-10 5 5 0 0 0 0 10Z'],
  person_add: ['M16 21a7 7 0 0 0-12 0', 'M10 13a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z', 'M19 8v6', 'M16 11h6'],
  phone: ['M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.4 19.4 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1 1 .4 2 .7 2.9a2 2 0 0 1-.5 2.1L8 8a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.5c.9.3 1.9.6 2.9.7a2 2 0 0 1 1.7 2Z'],
  progress_activity: ['M12 3v3', 'M12 18v3', 'M3 12h3', 'M18 12h3', 'm5.6 5.6 2.1 2.1', 'm16.3 16.3 2.1 2.1', 'm18.4 5.6-2.1 2.1', 'm7.7 16.3-2.1 2.1'],
  rule: ['M4 5h16', 'M4 12h16', 'M4 19h16', 'm7 4 1 1 2-2', 'm7 11 1 1 2-2', 'm7 18 1 1 2-2'],
  shield: ['M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z', 'm9 12 2 2 4-4'],
  support_agent: ['M4 13v-2a8 8 0 0 1 16 0v2', 'M4 13h3v6H6a2 2 0 0 1-2-2v-4Z', 'M20 13h-3v6h1a2 2 0 0 0 2-2v-4Z', 'M12 21h3'],
  verified: ['M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20Z', 'm7.5 12 3 3 6-6'],
  verified_user: ['M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z', 'm9 12 2 2 4-4'],
  visibility: ['M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z', 'M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z'],
  visibility_off: ['m3 3 18 18', 'M10.6 10.6a2 2 0 0 0 2.8 2.8', 'M9.9 5.2A11 11 0 0 1 12 5c6.5 0 10 7 10 7a16 16 0 0 1-3.1 3.8', 'M6.6 6.6C3.6 8.4 2 12 2 12s3.5 7 10 7c1 0 1.9-.2 2.7-.5'],
};

@Component({
  selector: 'app-auth-icon',
  standalone: true,
  template: `
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"
      stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">
      @for (path of paths; track path) {
        <path [attr.d]="path" />
      }
    </svg>
  `,
  styles: [`
    :host {
      display: inline-flex;
      width: 1.15em;
      height: 1.15em;
      flex: 0 0 auto;
      align-items: center;
      justify-content: center;
      vertical-align: middle;
    }
    svg {
      display: block;
      width: 100%;
      height: 100%;
    }
  `],
})
export class AuthIcon {
  @Input({ required: true }) name!: AuthIconName;

  get paths(): readonly string[] {
    return iconPaths[this.name];
  }
}
