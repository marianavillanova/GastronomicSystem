namespace RestaurantAPI.DTOs;
 // DTO for Enhanced Daily Report
public class EnhancedDailyReportDto
{
    public DateTime ReportDate { get; set; } // Report date
    public decimal? TotalIncome { get; set; } // Total revenue for the day
    public int? TotalOrders { get; set; } // Total number of orders
    public int? TotalPax { get; set; } // Total number of pax served
    public decimal? TotalDiscount { get; set; }
    public List<CategorySummaryDto> CategoryBreakdown { get; set; } = new(); // Breakdown by article category
    public List<PaymentMethodSummaryDto> PaymentBreakdown { get; set; } = new(); // Breakdown by payment method
}
