import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Order } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiUrl = `${environment.apiBaseUrl}/order`;
  private itemUrl = `${environment.apiBaseUrl}/orderitem`;

  constructor(private http: HttpClient) {}

  // 🔍 Get current order for a table (includes employee)
  getOrderByTableId(tableId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/table/${tableId}`);
  }

  // ➕ Add an item to the order
  addOrderItem(item: {
    tableId: number;
    articleId: number;
    quantity: number;
    price: number;
    comment?: string;
    discount?: number;
  }): Observable<any> {
    return this.http.post<any>(this.itemUrl, item);
  }

  // 📝 Update an existing order item
  submitOrder(orderId: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/${orderId}/submit`, {});
}

  // 🆕 Create a new order for a table
  createOrder(payload: { tableId: number; employeeId: number; paxAmount: number, globalDiscount?: number | null }): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/create`, payload);
  }

  // 🪑 Update table status when opening a table
  updateTableStatus(payload: {
    tableId: number;
    status: string;
    employeeId: number;
    paxAmount: number;
  }): Observable<any> {
    return this.http.put(`${environment.apiBaseUrl}/restauranttable/${payload.tableId}/open`, payload);
  }

  // 🔄 Get active order for a table
  getActiveOrderForTable(tableId: number): Observable<Order> {
  return this.http.get<Order>(`${this.apiUrl}/table/${tableId}`);
}

  // 👤 Assign a customer to an order
  assignCustomerToOrder(orderId: number, customerId: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/${orderId}/assign-customer`, customerId);
}

  // 🌍 Set global discount for an order
  setGlobalDiscount(orderId: number, discount: number): Observable<void> {
  return this.http.put<void>(`${this.apiUrl}/${orderId}/discount`, discount);
}

  // 🗑️ Delete an order item by its ID
  deleteOrderItem(orderItemId: number): Observable<void> {
  return this.http.delete<void>(`${this.itemUrl}/${orderItemId}`);
}

  //close table by order id
  closeTableByOrder(orderId: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/${orderId}/closeTable`, {});
}

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }
  
}
