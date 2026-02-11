namespace RestaurantAPI.DTOs;
// DTO for Payment Method Report
public class PaymentMethodReportDto
{
    public DateTime ReportDate { get; set; } // Report date
    public List<PaymentMethodSummaryDto> PaymentMethodBreakdown { get; set; } = new List<PaymentMethodSummaryDto>(); // Breakdown by payment method
}

