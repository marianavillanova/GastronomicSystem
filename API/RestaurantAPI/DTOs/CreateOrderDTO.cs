namespace RestaurantAPI.DTOs;
public class CreateOrderDto
{
    public int TableId { get; set; }
    public int EmployeeId { get; set; }
    public int PaxAmount { get; set; }
    public decimal? GlobalDiscount { get; set; }
}