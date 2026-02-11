namespace RestaurantAPI.DTOs;

public class CompanyInfo
{
    public int CustomerId { get; set; }
    public string CompanyName { get; set; } = null!;
    public string TaxNumber { get; set; } = null!;
}