import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { StartShiftDto, EnhancedDailyReportDto } from '../models/shift.model';

@Injectable({ providedIn: 'root' })
export class ShiftService {
  private apiUrl = `${environment.apiBaseUrl}/shift`;

  constructor(private http: HttpClient) {}

  getActiveShift(employeeId: number): Observable<any> {
  return this.http.get(`${this.apiUrl}/active?employeeId=${employeeId}`);
}

getDailyReportByDate(date: string): Observable<any> {
  return this.http.get(`${this.apiUrl}/report/date/${date}`);
}

startShift(payload: StartShiftDto): Observable<any> {
  return this.http.post(`${this.apiUrl}/start`, payload);
}


 // shift.service.ts
endShift(userId: number): Observable<EnhancedDailyReportDto> {
  return this.http.post<EnhancedDailyReportDto>(`${this.apiUrl}/end`, { userId });
}


downloadReportPdfByDate(date: string) {
  // date format: 'YYYY-MM-DD' or your backend’s expected format
  return this.http.get(`${this.apiUrl}/report/pdf/${date}`, {
    responseType: 'blob'
  });
}

getCurrentShift(): Observable<any> {
  return this.http.get(`${this.apiUrl}/current`);
}

}
