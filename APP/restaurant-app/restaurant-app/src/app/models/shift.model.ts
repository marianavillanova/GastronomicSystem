
export interface CategorySummaryDto {
  category: string;
  totalIncome: number | null;
  totalOrders: number;
}

export interface PaymentMethodSummaryDto {
  paymentMethod: string;
  totalRevenue: number;
  transactionCount: number;
}

export interface EnhancedDailyReportDto {
  reportDate: string; 
  totalIncome: number;
  totalOrders: number;
  totalPax: number;
  totalDiscount: number;   // 👈 new field
  categoryBreakdown: CategorySummaryDto[];
  paymentBreakdown: PaymentMethodSummaryDto[];
}


export interface StartShiftDto {
  userId: number;
}
