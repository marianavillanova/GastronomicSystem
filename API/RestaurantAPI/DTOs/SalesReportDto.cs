namespace RestaurantAPI.DTOs;
public class SalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalIncome { get; set; }
    public int TotalOrders { get; set; }
    public int TotalPax { get; set; }
    public decimal TotalDiscount { get; set; }
}
