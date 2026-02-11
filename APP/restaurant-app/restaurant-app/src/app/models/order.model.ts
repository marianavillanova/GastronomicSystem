import { Article } from './article.model';

export interface OrderItem {
  orderItemId?: number;
  articleId: number;
  name: string;
  price: number;
  quantity: number;
  comment?: string;
  discount?: number;
  article?: Article;
}

export interface Order {
  orderId: number;
  tableId: number;
  status: string;
  globalDiscount?: number;
  orderItems: OrderItem[] | { $values: OrderItem[] };
}
