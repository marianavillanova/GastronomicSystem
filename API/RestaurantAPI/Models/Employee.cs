using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string LoginCode { get; set; } = null!;

    public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

}
