namespace RestaurantAPI.DTOs;

// DTO for Category Breakdown
public class CategorySummaryDto
{
    public string Category { get; set; } = null!;
    public decimal? TotalIncome { get; set; }
    public int TotalOrders { get; set; }
}
