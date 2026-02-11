namespace RestaurantAPI.DTOs;
public class CreateOrderItemDto
{
    public int TableId { get; set; }
    public int ArticleId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Comment { get; set; }
    public decimal? Discount { get; set; }  // ✅ New property for discount
}
