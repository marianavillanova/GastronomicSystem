export interface Bill {
  id?: number;
  orderId: number;
  customerId?: number;
  totalAmount: number;
  paymentMethod: string;
  discount?: number;
  issueDate: Date;
}

export interface CreateBillDto {
  orderId: number;
  paymentMethod: 'cash' | 'card' | 'split';
  splitCashAmount?: number;
  splitCardAmount?: number;
  subtotal?: number;
  discount?: number;
}

export interface PaymentDetails {
  method: 'cash' | 'card' | 'split';
  customerType: 'final' | 'corporate' | null;
  splitCash?: number;
  splitCard?: number;
  corporateInfo?: {
    companyName: string;
    vatNumber: string;
    address: string;
    contact: string;
  };
}
