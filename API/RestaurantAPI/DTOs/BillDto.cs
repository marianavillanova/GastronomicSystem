public class BillDto
{
    public int BillId { get; set; }
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public DateTime IssueDate { get; set; }
    public decimal? Discount { get; set; }
    public decimal Subtotal { get; set; }
    public required string PaymentMethod { get; set; }
    public int? CustomerId { get; set; }
}