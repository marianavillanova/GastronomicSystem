import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Table } from '../models/table.model';
import { OpenTableDto } from '../models/open-table.dto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class TableService {
  private apiUrl = `${environment.apiBaseUrl}/restauranttable`;
  constructor(private http: HttpClient) {}

  // Fetch all tables
  getTables(): Observable<Table[]> {
    return this.http.get<Table[]>(this.apiUrl);
  }

  // Open a new table
  openTable(tableId: number, payload: OpenTableDto): Observable<Table> {
    return this.http.put<Table>(`${this.apiUrl}/${tableId}/open`, payload);
  }

  // Prepare a table
  prepareTable(tableId: number, dto: OpenTableDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${tableId}/prepare`, dto);
  }

  // Get table by ID
  getTableById(tableId: number): Observable<Table> {
    return this.http.get<Table>(`${this.apiUrl}/${tableId}`);
  }

  // Close a table
  closeTable(tableId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${tableId}`);
  }
}