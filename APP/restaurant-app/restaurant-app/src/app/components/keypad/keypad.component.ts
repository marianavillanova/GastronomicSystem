import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeService } from '../../services/employee.service';
import { Router } from '@angular/router';
import { SessionService } from '../../services/session.service';
import { AlertService } from '../../shared/alert/alert.service';

@Component({
  selector: 'app-keypad',
  imports: [CommonModule],
  templateUrl: './keypad.component.html',
  styleUrls: ['./keypad.component.scss']
})
export class KeypadComponent {
  inputValue: string = '';
  alertService = inject(AlertService);

  constructor(
    private employeeService: EmployeeService,
    private sessionService: SessionService,
    private router: Router
  ) {}

  // ✅ Listen for physical keyboard input
  @HostListener('window:keydown', ['$event'])
  handleKeyboardInput(event: KeyboardEvent): void {
    const key = event.key;

    if (/^\d$/.test(key)) {
      this.pressKey(key);
    } else if (key === 'Backspace') {
      this.deleteKey();
    } else if (key === 'Enter') {
      this.submit();
    }
  }

  pressKey(value: string): void {
    if (this.inputValue.length < 6) {
      this.inputValue += value;
    }
  }

  deleteKey(): void {
    this.inputValue = this.inputValue.slice(0, -1);
  }

  submit(): void {
    this.employeeService.login(this.inputValue).subscribe({
      next: (res) => {
        console.log('✅ Logged in as:', res.name);
        console.log('📦 Storing employee:', res);

        this.sessionService.setEmployee({
          employeeId: res.employeeId,
          role: res.role,
          name: res.name
        });

        this.router.navigate(['/tables']);
      },
      error: () => {
        this.alertService.showAlert('❌ Invalid login code. Please try again.');
      }
    });
  }
}
