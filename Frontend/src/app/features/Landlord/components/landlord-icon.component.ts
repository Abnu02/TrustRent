import { Component, Input } from '@angular/core';

export type LandlordIconName =
  | 'arrow-right'
  | 'buildings'
  | 'check'
  | 'check-circle'
  | 'clock'
  | 'close'
  | 'cloud-off'
  | 'dashboard'
  | 'home'
  | 'location'
  | 'log-out'
  | 'money'
  | 'plus'
  | 'plus-circle'
  | 'refresh'
  | 'shield'
  | 'shield-check'
  | 'x-circle';

const iconPaths: Record<LandlordIconName, readonly string[]> = {
  'arrow-right': ['M5 12h14', 'm12 5 7 7-7 7'],
  buildings: ['M3 21h18', 'M5 21V7l8-4v18', 'M19 21V11l-6-4', 'M8 9v.01', 'M8 12v.01', 'M8 15v.01', 'M8 18v.01', 'M16 13v.01', 'M16 16v.01', 'M16 19v.01'],
  check: ['m5 12 4 4L19 6'],
  'check-circle': ['M22 11.1V12a10 10 0 1 1-5.9-9.1', 'm9 11 3 3L22 4'],
  clock: ['M12 8v4l3 2', 'M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0Z'],
  close: ['M18 6 6 18', 'm6 6 12 12'],
  'cloud-off': ['m2 2 20 20', 'M5.6 5.6A7 7 0 0 1 19 9', 'M5 9a5 5 0 0 0 0 10h11', 'M16.5 19H19a3 3 0 0 0 1.7-5.5'],
  dashboard: ['M3 3h8v8H3z', 'M13 3h8v5h-8z', 'M13 10h8v11h-8z', 'M3 13h8v8H3z'],
  home: ['m3 10 9-7 9 7', 'M5 9v12h14V9', 'M9 21v-7h6v7'],
  location: ['M20 10c0 5-8 12-8 12S4 15 4 10a8 8 0 1 1 16 0Z', 'M12 10a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5Z'],
  'log-out': ['M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4', 'm16 17 5-5-5-5', 'M21 12H9'],
  money: ['M12 2v20', 'M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6'],
  plus: ['M12 5v14', 'M5 12h14'],
  'plus-circle': ['M12 8v8', 'M8 12h8', 'M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0Z'],
  refresh: ['M20 7v5h-5', 'M4 17v-5h5', 'M5.6 9A7 7 0 0 1 17 6l3 6', 'M18.4 15A7 7 0 0 1 7 18l-3-6'],
  shield: ['M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z'],
  'shield-check': ['M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z', 'm9 12 2 2 4-4'],
  'x-circle': ['M15 9 9 15', 'm9 9 6 6', 'M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0Z'],
};

@Component({
  selector: 'app-landlord-icon',
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
    :host { display: inline-flex; width: 1.2em; height: 1.2em; flex: 0 0 auto; align-items: center; justify-content: center; vertical-align: middle; }
    svg { display: block; width: 100%; height: 100%; }
  `],
})
export class LandlordIconComponent {
  @Input({ required: true }) name!: LandlordIconName;

  get paths(): readonly string[] {
    return iconPaths[this.name];
  }
}
