using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public partial class Orders
{
    public int OrderId { get; set; }

    public int TableId { get; set; }

    public int EmployeeId { get; set; }

    public int? CustomerId { get; set; }

    public int PaxAmount { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;
    
    public decimal? GlobalDiscount { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual RestaurantTable Table { get; set; } = null!;

    public virtual Customer? Customer { get; set; } 
}
