import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

// Adjust these interfaces to match your backend DTOs
export interface SalesReportDto {
  startDate: string;
  endDate: string;
  totalIncome: number;
  totalOrders: number;
  totalPax: number;
  totalDiscount: number;
}

export interface MostSoldArticlesDto {
  articleName: string;
  category: string;
  quantitySold: number;
  totalIncome: number;
}

export interface CustomerTypeSummaryDto {
  customerType: string;
  totalRevenue: number;
  transactionCount: number;
}

export interface CustomerTypeReportDto {
  reportDate?: string;
  startDate?: string;
  endDate?: string;
  customerTypeBreakdown: CustomerTypeSummaryDto[];
}

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = `${environment.apiBaseUrl}/dailyreport`; 

  constructor(private http: HttpClient) {}

  getSalesReport(startDate: string, endDate: string): Observable<SalesReportDto> {
    return this.http.get<SalesReportDto>(
      `${this.apiUrl}/sales?startDate=${startDate}&endDate=${endDate}`
    );
  }

  getMostSoldArticles(startDate: string, endDate: string): Observable<MostSoldArticlesDto[]> {
  return this.http
    .get<any>(`${this.apiUrl}/mostsold?startDate=${startDate}&endDate=${endDate}`)
    .pipe(map((res: any) => res.$values ?? []));
}

getCustomerTypes(startDate: string, endDate: string): Observable<CustomerTypeReportDto> {
  return this.http
    .get<any>(`${this.apiUrl}/customertypes?startDate=${startDate}&endDate=${endDate}`)
    .pipe(
      map((res: any) => ({
        ...res,
        customerTypeBreakdown: res.customerTypeBreakdown?.$values ?? []
      }))
    );
}

}
