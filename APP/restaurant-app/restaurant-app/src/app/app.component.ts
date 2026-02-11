import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AlertService } from './shared/alert/alert.service';
import { AlertModalComponent } from './shared/alert/alert-modal.component';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AlertModalComponent, AsyncPipe],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})

export class AppComponent {
  title = 'restaurant-app';

  alertService = inject(AlertService);
}
