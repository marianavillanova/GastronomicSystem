public class CreateBillDto
{
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } = "cash"; // or "card", "split"
    public decimal? SplitCashAmount { get; set; }
    public decimal? SplitCardAmount { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? Discount { get; set; }

}
