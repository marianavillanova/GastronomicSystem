import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SessionService {
  employee: { employeeId: number; role: string; name: string } | null = null;

  setEmployee(emp: { employeeId: number; role: string; name: string }) {
    this.employee = emp;
    localStorage.setItem('employee', JSON.stringify(emp));
  }

 getEmployeeId(): number | null {
  if (!this.employee) {
    const stored = localStorage.getItem('employee');
    if (stored) {
      this.employee = JSON.parse(stored);
    }
  }
  return this.employee?.employeeId ?? null;
}

getEmployeeName(): string {
  if (!this.employee) {
    const stored = localStorage.getItem('employee');
    if (stored) {
      this.employee = JSON.parse(stored);
    }
  }
  if (!this.employee?.name) {
    throw new Error('Employee name is missing from session');
  }

  return this.employee.name;
}

getEmployeeRole(): string | null {
  if (!this.employee) {
    const stored = localStorage.getItem('employee');
    if (stored) {
      this.employee = JSON.parse(stored);
    }
  }
  return this.employee?.role ?? null;
}


}
