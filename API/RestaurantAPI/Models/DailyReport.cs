using System;
using System.Collections.Generic;

namespace RestaurantAPI.Models;

public class DailyReport
{
    public int ReportId { get; set; }
    public DateTime ReportDate { get; set; }
    public decimal? TotalIncome { get; set; }
    public int? TotalOrders { get; set; }
    public int? TotalPax { get; set; }
    public int ShiftStartUserId { get; set; }
    public int? ShiftEndUserId { get; set; }
    public string ShiftStatus { get; set; } = "Open";
    public DateTime ShiftStartTime { get; set; }
    public DateTime? ShiftEndTime { get; set; }
    public decimal? TotalDiscount { get; set; }
}

