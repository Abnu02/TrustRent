import { Component, input } from '@angular/core';

export type AdminIconName =
  | 'overview'
  | 'property'
  | 'landlords'
  | 'audit'
  | 'search'
  | 'notifications'
  | 'arrow'
  | 'check'
  | 'close'
  | 'document'
  | 'bolt'
  | 'image'
  | 'shield'
  | 'clock'
  | 'chevron'
  | 'menu';

@Component({
  selector: 'app-admin-icon',
  standalone: true,
  template: `
    <svg
      aria-hidden="true"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      stroke-width="1.8"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      @switch (name()) {
        @case ('overview') {
          <rect x="3" y="3" width="8" height="8" rx="1.5" />
          <rect x="13" y="3" width="8" height="5" rx="1.5" />
          <rect x="13" y="10" width="8" height="11" rx="1.5" />
          <rect x="3" y="13" width="8" height="8" rx="1.5" />
        }
        @case ('property') {
          <path d="m3 10 9-7 9 7" />
          <path d="M5 9v11h14V9M9 20v-7h6v7" />
        }
        @case ('landlords') {
          <path d="M16 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
          <circle cx="10" cy="7" r="4" />
          <path d="M20 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
        }
        @case ('audit') {
          <path d="M8 3h8l4 4v14H4V3h4Z" />
          <path d="M8 11h8M8 15h8M8 7h3" />
        }
        @case ('search') {
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-4-4" />
        }
        @case ('notifications') {
          <path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9" />
          <path d="M10 21h4" />
        }
        @case ('arrow') {
          <path d="M5 12h14m-6-6 6 6-6 6" />
        }
        @case ('check') {
          <path d="m5 12 4 4L19 6" />
        }
        @case ('close') {
          <path d="m18 6-12 12M6 6l12 12" />
        }
        @case ('document') {
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z" />
          <path d="M14 2v6h6M8 13h8M8 17h8" />
        }
        @case ('bolt') {
          <path d="m13 2-3 9h7l-6 11 2-9H6l7-11Z" />
        }
        @case ('image') {
          <rect x="3" y="3" width="18" height="18" rx="2" />
          <circle cx="8.5" cy="8.5" r="1.5" />
          <path d="m21 15-5-5L5 21" />
        }
        @case ('shield') {
          <path d="M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z" />
          <path d="m9 12 2 2 4-4" />
        }
        @case ('clock') {
          <circle cx="12" cy="12" r="9" />
          <path d="M12 7v5l3 2" />
        }
        @case ('chevron') {
          <path d="m9 18 6-6-6-6" />
        }
        @case ('menu') {
          <path d="M4 6h16M4 12h16M4 18h16" />
        }
      }
    </svg>
  `,
  styles: `
    :host {
      display: inline-flex;
      width: 1.15rem;
      height: 1.15rem;
      flex: 0 0 auto;
    }

    svg {
      width: 100%;
      height: 100%;
    }
  `,
})
export class AdminIcon {
  readonly name = input.required<AdminIconName>();
}
