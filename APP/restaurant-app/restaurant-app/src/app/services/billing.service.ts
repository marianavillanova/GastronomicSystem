import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Bill } from '../models/bill.model';
import { environment } from '../../environments/environment';
import { CreateBillDto } from './../models/bill.model';

@Injectable({
  providedIn: 'root',
})
export class BillingService {
  private apiUrl = `${environment.apiBaseUrl}/bill`;
  

  constructor(private http: HttpClient) {}

  // Fetch all bills
  getBills(): Observable<Bill[]> {
    return this.http.get<Bill[]>(this.apiUrl);
  }

  // Create a new bill
  createBill(payload: CreateBillDto): Observable<Bill> {
  return this.http.post<Bill>(this.apiUrl, payload);
}

  // Fetch a bill by ID
  getBillById(billId: number): Observable<Bill> {
    return this.http.get<Bill>(`${this.apiUrl}/${billId}`);
  }

}