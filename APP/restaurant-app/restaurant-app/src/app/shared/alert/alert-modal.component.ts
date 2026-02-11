import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlertType } from './alert.service';

@Component({
  selector: 'app-alert-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './alert-modal.component.html',
  styleUrls: ['./alert-modal.component.scss']
})
export class AlertModalComponent {
  @Input() title = 'Notice';
  @Input() message = '';
  @Input() type: AlertType = 'warning';
  @Output() close = new EventEmitter<void>();

  onClose() {
    this.close.emit();
  }

  get icon(): string {
    switch (this.type) {
      case 'success': return '✔';
      case 'info': return 'ℹ';
      case 'error': return '✖';
      default: return '⚠';
    }
  }
}
