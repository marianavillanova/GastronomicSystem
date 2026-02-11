using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int ArticleId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string? Comment { get; set; }
    
    public int TableId { get; set; }  // ✅ Makes it explicit which table the item belongs to

    public decimal? Discount { get; set; }
    
    public virtual Article Article { get; set; } = null!;

    public virtual Orders Order { get; set; } = null!;
}
