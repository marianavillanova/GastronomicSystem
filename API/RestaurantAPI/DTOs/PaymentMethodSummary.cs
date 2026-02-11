namespace RestaurantAPI.DTOs;
// DTO for Payment Method Breakdown
public class PaymentMethodSummaryDto
{
    public string PaymentMethod { get; set; } = null!; // Payment method name (e.g., Cash, Card)
    public decimal TotalRevenue { get; set; } // Total revenue from this payment method
    public int TransactionCount { get; set; } // Number of transactions using this payment method
}
