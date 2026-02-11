namespace RestaurantAPI.DTOs;

public class PaymentRequest
{
    public decimal TotalPaid { get; set; }
    public List<string> Methods { get; set; } = new List<string>(); // E.g., ["Cash", "Card"]
    public string CustomerType { get; set; } = null!; // Corporate or Final
    public CompanyInfo? CompanyInfo { get; set; } // Optional: Only for Corporate customers
}
