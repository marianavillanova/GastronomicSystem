using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public partial class Article
{
    public int ArticleId { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? SubCategory { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true; // New field to indicate active status

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
