import { Component, OnInit, inject } from '@angular/core';
import { TableService } from '../../services/table.service';
import { Table } from '../../models/table.model';
import { Router } from '@angular/router'; 
import { SessionService } from '../../services/session.service';
import { OpenTableDto } from '../../models/open-table.dto';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SidebarMenuComponent } from '../sidebar-menu/sidebar-menu.component';
import { AlertService } from '../../shared/alert/alert.service';

@Component({
  selector: 'app-table-management',
  templateUrl: './table-management.component.html',
  imports: [CommonModule, FormsModule, SidebarMenuComponent],
  styleUrls: ['./table-management.component.scss']
})

export class TableManagementComponent implements OnInit {
  tables: Table[] = [];
  showGuestPrompt = false;
  manualInputEnabled = false;
  manualGuestCount: number | null = null;
  quickGuestOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  selectedTable: Table | null = null;
  employeeId!: number;
  role!: string;
  alertService = inject(AlertService);

  constructor(
    private tableService: TableService, 
    private router: Router,
    private sessionService: SessionService
  ) {}

  ngOnInit(): void {
    const vh = window.innerHeight * 0.01;
    document.documentElement.style.setProperty('--vh', `${vh}px`);

    const id = this.sessionService.getEmployeeId();
    const role = this.sessionService.getEmployeeRole();
    console.log('🧭 Retrieved employee ID:', id, 'Role', role);
    if (id === null || !role) {
      this.alertService.showAlert('No employee ID found. Please log in again.');
      this.router.navigate(['/keypad']);
      return;
    }

    this.employeeId = id;
    this.role = role.toLocaleLowerCase();
    this.loadTables();
  }

  loadTables(): void {
    this.tableService.getTables().subscribe({
      next: (data) => {
        console.log('📦 Full backend response:', data);
        this.tables = (data as any).$values;
        console.log('🪑 Parsed tables:', this.tables);
      }
    });
  }
  
  isElevatedRole(): boolean {
  return ['manager', 'supervisor', 'head waiter'].includes(this.role);
}


  handleTableClick(table: Table): void {
    if (!table.status) {
      this.selectedTable = table;
      this.showGuestPrompt = true;
      this.manualInputEnabled = false;
      this.manualGuestCount = null;
    } else if (
      table.employeeId === this.employeeId ||
      ['manager', 'supervisor', 'head waiter'].includes(this.role)
    ) {
      this.onTableSelect(table);
    } else {
     this.alertService.showAlert(`Table ${table.tableNumber} is occupied by another employee.`);
    }


  }

  selectGuestCount(count: number): void {
    if (this.selectedTable) {
      this.prepareTable(this.selectedTable, count);
      this.showGuestPrompt = false;
    }
  }

  enableManualInput(): void {
    this.manualInputEnabled = true;
  }

  confirmManualCount(): void {
    if (this.manualGuestCount && this.manualGuestCount > 0 && this.selectedTable) {
      this.prepareTable(this.selectedTable, this.manualGuestCount);
      this.showGuestPrompt = false;
    }
  }

  closeGuestPrompt(): void {
  this.showGuestPrompt = false;
  this.manualInputEnabled = false;
  this.manualGuestCount = null;
  this.selectedTable = null;
}


  prepareTable(table: Table, pax: number): void {
      console.log('🚀 prepareTable called with:', table.tableId, pax);

      const dto: OpenTableDto = {
        employeeId: this.employeeId,
        paxAmount: pax
      };

      this.tableService.prepareTable(table.tableId, dto).subscribe({
        next: () => {
          console.log('✅ Backend responded, navigating to menu...');
          this.loadTables();
          this.router.navigate(['/menu', table.tableId], {
            queryParams: { paxAmount: pax }
          });
        },
        error: (err) => {
          console.error('❌ Backend error during prepareTable:', err);
        }
      });
    }



  closeTable(tableId: number): void {
    this.tableService.closeTable(tableId).subscribe(() => {
      this.loadTables();
    });
  }

  onTableSelect(table: Table): void {
    const isOccupiedBySomeoneElse =
      !table.status && table.employeeId !== this.employeeId;

    if (isOccupiedBySomeoneElse) {
     this.alertService.showAlert('This table is already being served by another employee.');
      return;
    }

    this.router.navigate(['/menu', table.tableId]);
  }

  goBack(): void {
  this.router.navigate(['/']);
}


  getTablePosition(tableNumber: number): { top: string; left: string } {
    const isPWA = window.matchMedia('(display-mode: standalone)').matches;

    const browserPositions: Record<number, { top: string; left: string }> = {
      // Group A
      1: { top: '13.7%', left: '15.7%' },
      2: { top: '36.6%', left: '15.7%' },
      3: { top: '59.5%', left: '15.7%' },
      4: { top: '82.3%', left: '15.7%' },

      5: { top: '13.7%', left: '26.2%' },
      6: { top: '36.6%', left: '26.2%' },
      7: { top: '59.5%', left: '26.2%' },
      8: { top: '82.3%', left: '26.2%' },

      // Group B
      9:  { top: '36.6%', left: '42.2%' },
      10: { top: '59.5%', left: '42.2%' },
      11: { top: '82.3%', left: '42.2%' },

      12: { top: '36.6%', left: '52.7%' },
      13: { top: '59.5%', left: '52.7%' },
      14: { top: '82.3%', left: '52.7%' },

      // Group C
      15: { top: '36.6%', left: '68.7%' },
      16: { top: '59.5%', left: '68.7%' },
      17: { top: '82.3%', left: '68.7%' },

      18: { top: '36.6%', left: '79%' },
      19: { top: '59.5%', left: '79%' },
      20: { top: '82.3%', left: '79%' },
    };

    const pwaPositions: Record<number, { top: string; left: string }> = {
      // Adjusted manually for PWA display
      1: { top: '18%', left: '15.7%' },
      2: { top: '38%', left: '15.7%' },
      3: { top: '58.4%', left: '15.7%' },
      4: { top: '78.6%', left: '15.7%' },

      5: { top: '18%', left: '26.25%' },
      6: { top: '38%', left: '26.25%' },
      7: { top: '58.4%', left: '26.25%' },
      8: { top: '78.6%', left: '26.25%' },

      9:  { top: '38%', left: '42.3%' },
      10: { top: '58.4%', left: '42.3%' },
      11: { top: '78.6%', left: '42.3%' },

      12: { top: '38%', left: '52.7%' },
      13: { top: '58.4%', left: '52.7%' },
      14: { top: '78.6%', left: '52.7%' },

      15: { top: '38%', left: '68.7%' },
      16: { top: '58.4%', left: '68.7%' },
      17: { top: '78.6%', left: '68.7%' },

      18: { top: '38%', left: '79.1%' },
      19: { top: '58.4%', left: '79.1%' },
      20: { top: '78.6%', left: '79.1%' },
    };

    const positions = isPWA ? pwaPositions : browserPositions;
    return positions[tableNumber] || { top: '0%', left: '0%' };
  }

}
