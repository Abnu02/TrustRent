import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output
} from '@angular/core';
import { DecimalPipe } from '@angular/common';

import { Property } from '../../../../core/models/property.model';

@Component({
  selector: 'app-property-card',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './property-card.component.html',
  styleUrl: './property-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PropertyCardComponent {

  @Input({ required: true })
  property!: Property;

  @Output()
  viewDetails = new EventEmitter<string>();

}