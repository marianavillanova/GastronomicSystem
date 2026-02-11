import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private apiUrl = `${environment.apiBaseUrl}/Employee`;
  
  constructor(private http: HttpClient) {}

  login(pin: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post(`${this.apiUrl}/login`, JSON.stringify(pin), { headers }); //json.stringify para que la api reciba info raw
  } 
    
}
