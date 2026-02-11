import { Component, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { SessionService } from '../../services/session.service';
import { ShiftService } from '../../services/shift.service';
import { StartShiftDto, EnhancedDailyReportDto, PaymentMethodSummaryDto } from '../../models/shift.model';
import { Router } from '@angular/router';
import { AlertService } from '../../shared/alert/alert.service';

@Component({
  selector: 'app-session-control',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './session-control.component.html',
  styleUrls: ['./session-control.component.scss']
})


export class SessionControlComponent implements OnInit {
  sessionActive = false;
  sessionUser = '';
  sessionStartTime = '';
  employeeId!: number;

  alertService = inject(AlertService);
  currentReport?: EnhancedDailyReportDto;

  constructor(
    private http: HttpClient,
    private sessionService: SessionService,
    private shiftService: ShiftService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.sessionService.getEmployeeId();
    const name = this.sessionService.getEmployeeName();

    if (id === null || !name) {
     this.alertService.showAlert('No employee info found. Please log in again.');
      return;
    }

    this.employeeId = id;
    this.sessionUser = name;

    this.shiftService.getActiveShift(this.employeeId).subscribe({
      next: (shift) => {
        if (shift) {
          this.sessionActive = true;
          this.sessionStartTime = new Date(shift.startTime).toLocaleTimeString();
        } else {
          this.sessionActive = false;
        }
      },
      error: (err) => {
        console.error('Unexpected error while checking shift:', err);
        this.sessionActive = false;
      }
    });
  }

  startSession(): void {
    const payload: StartShiftDto = { userId: this.employeeId };
    this.shiftService.startShift(payload).subscribe({
      next: () => {
        this.sessionActive = true;
        this.sessionStartTime = new Date().toLocaleTimeString();
      },
      error: (err) => {
       this.alertService.showAlert(err.error);
      }
    });
  }

  // End shift: refresh UI with JSON response
    endSession(): void {
      this.shiftService.endShift(this.employeeId).subscribe({
        next: (report) => {
          console.log('Shift ended. Daily report:', report);

          this.currentReport = {
            ...report,
            paymentBreakdown: (report.paymentBreakdown as any)?.$values ?? [],
            categoryBreakdown: (report.categoryBreakdown as any)?.$values ?? []
          };
          this.sessionActive = false; // shift ended
        },
        error: (err) => {
         this.alertService.showAlert(typeof err.error === 'string' ? err.error : JSON.stringify(err.error));
        }
      });
    }

    trackByPaymentMethod(index: number, item: PaymentMethodSummaryDto): string {
    return item.paymentMethod;
  }
  
  goBack(): void {
    this.router.navigate(['/tables']);
  }

  // Manual PDF download on demand
  downloadPdf(reportDate: string | Date | undefined): void {
    if (!reportDate) {
     this.alertService.showAlert('No report available to download yet.');
      return;
    }
    const dateStr = typeof reportDate === 'string'
      ? reportDate
      : new Date(reportDate).toISOString().slice(0, 10); // YYYY-MM-DD

    this.shiftService.downloadReportPdfByDate(dateStr).subscribe({
      next: (blob) => {
        const fileName = `DailyReport_${dateStr.replace(/-/g, '')}.pdf`;
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
       this.alertService.showAlert('Failed to download PDF: ' + (err?.error ?? 'Unknown error'));
      }
    });
  }
}
