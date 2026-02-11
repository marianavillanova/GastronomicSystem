using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = null!;

    public string CustomerType { get; set; } = null!;

    public string? Contact { get; set; }

    public string? VatNumber { get; set; }

    public string? Address { get; set; }
    
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
