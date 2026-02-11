export interface Customer {
  customerId?: number;
  name: string;
  customerType: 'final' | 'corporate';
  contact?: string;
  vatNumber?: string;
  address?: string;
}
