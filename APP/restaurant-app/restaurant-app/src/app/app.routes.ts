import { Routes } from '@angular/router';
import { WelcomeScreenComponent } from './components/welcome-screen/welcome-screen.component';
import { KeypadComponent } from './components/keypad/keypad.component';
import { MenuComponent } from './components/menu/menu.component';
import { TableManagementComponent } from './components/table-management/table-management.component';
import { ReportsComponent } from './components/reports/reports.component'; 
import { SessionControlComponent } from './components/session-control/session-control.component';
import { DeleteBillsComponent } from './components/delete-bills/delete-bills.component';
import { AdminPanelComponent } from './components/admin-panel/admin-panel.component';
import { ReportService } from './services/report.service';

export const routes: Routes = [
  { path: '', component: WelcomeScreenComponent }, // Pagina de inicio
  { path: 'keypad', component: KeypadComponent  }, // Login con el keypad
  { path: 'tables', component: TableManagementComponent }, //Mapa de mesas
  { path: 'menu/:tableId', component: MenuComponent }, //Menu dinamico de la mesa

  {path: 'reports', component: ReportsComponent}, // reportes
  {path: 'session-control', component: SessionControlComponent}, // turnos
  {path: 'delete-bills', component: DeleteBillsComponent}, // bills
  {path: 'admin-panel', component: AdminPanelComponent} //panel 
];