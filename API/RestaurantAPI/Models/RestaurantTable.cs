using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public partial class RestaurantTable
{
     public int TableId { get; set; }

    public int TableNumber { get; set; }

    public int Capacity { get; set; }

    public bool Status { get; set; }

    public int? Pax { get; set; }  

    public int? EmployeeId { get; set; }  // ✅ Foreign Key

    public virtual Employee? Employee { get; set; } // ✅ Navigation Property

    public virtual ICollection<Orders> Order { get; set; } = new List<Orders>();
}


