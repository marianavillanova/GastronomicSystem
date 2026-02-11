using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public decimal Total { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal? Discount { get; set; }

    public decimal? SplitCashAmount { get; set; } = null;

    public decimal? SplitCardAmount { get; set; } = null;

    public decimal Subtotal { get; set; }

    public DateTime IssueDate { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Orders Order { get; set; } = null!;
}
