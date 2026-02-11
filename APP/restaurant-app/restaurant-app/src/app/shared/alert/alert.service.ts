import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type AlertType = 'warning' | 'info' | 'success' | 'error';

export interface AlertState {
  title?: string;
  message: string;
  type?: AlertType;
  confirmText?: string; // optional if you want confirmation-style
  showCancel?: boolean; // optional
}

@Injectable({ providedIn: 'root' })
export class AlertService {
  private readonly _alert$ = new BehaviorSubject<AlertState | null>(null);
  alert$ = this._alert$.asObservable();

  // Show a simple alert
  showAlert(message: string, type: AlertType = 'warning', title = 'Notice') {
    this._alert$.next({ title, message, type });
  }

  // Clear current alert
  close() {
    this._alert$.next(null);
  }
}
