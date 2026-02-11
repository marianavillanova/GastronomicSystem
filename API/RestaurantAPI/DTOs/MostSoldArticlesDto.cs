namespace RestaurantAPI.DTOs;

// DTO for Most Sold Articles Report
public class MostSoldArticlesDto
{
    public string ArticleName { get; set; } = null!; // Article name
   public int QuantitySold { get; set; } // Total quantity sold
    public decimal TotalIncome { get; set; } // Revenue generated from this article
}
