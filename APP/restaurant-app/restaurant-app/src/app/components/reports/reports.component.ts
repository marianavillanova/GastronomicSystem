import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService } from '../../services/report.service';
import { AlertService } from '../../shared/alert/alert.service';
import { SalesReportDto, MostSoldArticlesDto, CustomerTypeReportDto } from '../../services/report.service';
import { NgChartsModule } from 'ng2-charts';
import { Chart, registerables } from 'chart.js';
import { Router } from '@angular/router';

Chart.register(...registerables); 
Chart.defaults.color = '#ffffff';

type ReportTab = 'summary' | 'mostSold' | 'customerTypes';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, NgChartsModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss'],
})
export class ReportsComponent {
  private reportService = inject(ReportService);
  private alertService = inject(AlertService);

  constructor(private router: Router) {}
  // UI state
  selectedTab = signal<ReportTab>('summary');
  startDate = signal<string>('');   // new
  endDate = signal<string>('');     // new

  // Data state
  salesReport = signal<SalesReportDto | null>(null);
  mostSold = signal<MostSoldArticlesDto[]>([]);
  customerTypes = signal<CustomerTypeReportDto | null>(null);

  // Chart data for Most Sold Articles
  mostSoldLabels = signal<string[]>([]);
  mostSoldData = signal<number[]>([]);

  // Chart data for Customer Types
  customerTypeLabels = signal<string[]>([]);
  customerTypeData = signal<number[]>([]);


  // Actions
  selectTab(tab: ReportTab) {
    this.selectedTab.set(tab);
    if (this.startDate() && this.endDate()) {
      this.loadRangeReports();
    }
  }


  loadRangeReports() {
    const start = this.startDate();
    const end = this.endDate();
    const tab = this.selectedTab();

    if (!start || !end) {
      this.alertService.showAlert('Please select both start and end dates', 'warning', 'Missing Dates');
      return;
    }

    if (tab === 'summary') {
      this.reportService.getSalesReport(start, end).subscribe({
        next: (res) => this.salesReport.set(res),
        error: (err) => {
          console.error('Sales report load failed:', err);
          this.alertService.showAlert('Failed to load sales report', 'error', 'Load Error');
        }
      });
   } else if (tab === 'mostSold') {
  this.reportService.getMostSoldArticles(start, end).subscribe({
    next: res => {
      this.mostSold.set(res);

      // ⭐ Group by article name (simple and stable)
      const grouped = res.reduce((acc, item) => {
        const name = item.articleName;
        if (!acc[name]) acc[name] = 0;
        acc[name] += item.quantitySold;
        return acc;
      }, {} as Record<string, number>);

      // ⭐ Chart data
      this.mostSoldLabels.set(Object.keys(grouped));
      this.mostSoldData.set(Object.values(grouped));
    },
    error: () =>
      this.alertService.showAlert(
        'Failed to load most sold articles',
        'error',
        'Load Error'
      ),
  });

    } else if (tab === 'customerTypes') {
      this.reportService.getCustomerTypes(start, end).subscribe({
        next: res => {
          this.customerTypes.set(res);

          const breakdown = res.customerTypeBreakdown;

          // Build chart data
          this.customerTypeLabels.set(breakdown.map(ct => ct.customerType));
          this.customerTypeData.set(breakdown.map(ct => ct.totalRevenue));
        },
        error: () => this.alertService.showAlert('Failed to load customer types', 'error', 'Load Error'),
      });
    }
  }

  trackByArticle(index: number, item: MostSoldArticlesDto) {
    return item.articleName; // para trackear los cambios de la lista
  }

    updateStartDate(value: string) {
    this.startDate.set(value);
    this.salesReport.set(null);
    this.mostSold.set([]);
    this.customerTypes.set(null);
  }

  updateEndDate(value: string) {
    this.endDate.set(value);
    this.salesReport.set(null);
    this.mostSold.set([]);
    this.customerTypes.set(null);
  }

  goBack(): void {
    this.router.navigate(['/tables']);
  }
}
