using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs;
using RestaurantAPI.Models;


namespace RestaurantAPI.Repositories;

public class ShiftRepository
{
    private readonly GastronomicSystemContext _context;

    public ShiftRepository(GastronomicSystemContext context)
    {
        _context = context;
    }

    // ✅ Get active shift for an employee
    public Shift? GetActiveShift(int employeeId)
    {
        return _context.Shifts.FirstOrDefault(s => s.EmployeeId == employeeId && s.EndTime == null);
    }

    // ✅ Get current ongoing shift for the guard
       public async Task<DailyReport?> GetCurrentShiftAsync()
    {
        return await _context.DailyReports
            .Where(dr => dr.ShiftEndTime == null)
            .OrderByDescending(dr => dr.ShiftStartTime)
            .FirstOrDefaultAsync();
    }


    // ✅ Retrieve shift history
    public List<Shift> GetShiftHistory(int employeeId)
    {
        return _context.Shifts.Where(s => s.EmployeeId == employeeId).ToList();
    }

    // ✅ Save a new shift
    public void Save(Shift shift)
    {
        _context.Shifts.Add(shift);
        _context.SaveChanges();
    }

    // ✅ End a shift & update timestamp
    public void Update(Shift shift)
    {
        _context.Shifts.Update(shift);
        _context.SaveChanges();
    }

    public async Task<DailyReport?> GetDailyReportByIdAsync(int id)
    {
        return await _context.DailyReports.FindAsync(id);  // ✅ Correct database call
    }

    public async Task<DailyReport?> GetDailyReportByDateAsync(DateTime date)
    {
        return await _context.DailyReports
            .Where(dr => dr.ReportDate.Date == date.Date)
            .OrderByDescending(dr => dr.ShiftStartTime)
            .AsQueryable()  // ✅ Convert the ordered query to IQueryable before using FirstOrDefaultAsync
            .FirstOrDefaultAsync();
    }

    public async Task<List<PaymentMethodSummaryDto>> GetPaymentMethodBreakdownAsync(DateTime start, DateTime end)
    {
        var bills = await _context.Bills
            .Where(b => b.IssueDate >= start && b.IssueDate <= end)
            .ToListAsync();

        if (!bills.Any())
            return new List<PaymentMethodSummaryDto>();

        var breakdown = new Dictionary<string, (decimal revenue, int count)>();

        foreach (var bill in bills)
        {
            if (bill.PaymentMethod?.Equals("split", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var cash = bill.SplitCashAmount ?? 0;
                    var card = bill.SplitCardAmount ?? 0;

                    if (cash == 0 && card == 0)
                    {
                        var half = bill.Total / 2;
                        AddToBreakdown(breakdown, "Cash", half);
                        AddToBreakdown(breakdown, "Card", bill.Total - half);
                    }
                    else
                    {
                        if (cash > 0) AddToBreakdown(breakdown, "Cash", cash);
                        if (card > 0) AddToBreakdown(breakdown, "Card", card);
                    }
                }
                else
                {
                    AddToBreakdown(breakdown, bill.PaymentMethod ?? "Unknown", bill.Total);
                }
        }

        return breakdown.Select(kvp => new PaymentMethodSummaryDto
        {
            PaymentMethod = kvp.Key,
            TotalRevenue = kvp.Value.revenue,
            TransactionCount = kvp.Value.count
        }).ToList();
    }

    private void AddToBreakdown(Dictionary<string, (decimal revenue, int count)> breakdown, string method, decimal amount)
    {
        if (!breakdown.ContainsKey(method))
            breakdown[method] = (0, 0);

        breakdown[method] = (
            breakdown[method].revenue + amount,
            breakdown[method].count + 1
        );
    }




    public async Task<List<MostSoldArticlesDto>> GetMostSoldArticlesAsync(DateTime date)
    {
        var shiftRecord = await _context.DailyReports
            .Where(dr => dr.ReportDate.Date == date.Date)
            .OrderByDescending(dr => dr.ShiftStartTime)
            .FirstOrDefaultAsync();

        if (shiftRecord == null)
        {
            return new List<MostSoldArticlesDto>(); // ✅ Return an empty list if no shift record found
        }

        var bills = await _context.Bills
            .Where(b => b.IssueDate >= shiftRecord.ShiftStartTime
                && (shiftRecord.ShiftEndTime == null || b.IssueDate <= shiftRecord.ShiftEndTime))
            .Include(b => b.Order)
            .ThenInclude(o => o.OrderItems)
            .ThenInclude(oi => oi.Article)
            .ToListAsync();

        if (!bills.Any())
        {
            return new List<MostSoldArticlesDto>(); // ✅ Avoid returning null, return an empty list
        }

        return bills
            .Where(b => b.Order != null)
            .SelectMany(b => b.Order.OrderItems)
            .Where(oi => oi.Article != null)
            .GroupBy(oi => oi.ArticleId)
            .Select(g => new MostSoldArticlesDto
            {
                ArticleName = g.FirstOrDefault()?.Article?.Name ?? "Unknown", // ✅ Handle possible null values
                QuantitySold = g.Sum(oi => oi.Quantity),
                TotalIncome = g.Sum(oi => oi.Quantity * oi.Price)
            })
            .OrderByDescending(dto => dto.QuantitySold)
            .ToList();
    }

    public async Task<List<CustomerTypeSummaryDto>> GetCustomerTypeBreakdownAsync(DateTime date)
    {
        var bills = await _context.Bills
            .Where(b => b.IssueDate.Date == date.Date)
            .Include(b => b.Customer) // ✅ Ensure customers are loaded
            .ToListAsync();

        if (!bills.Any())
        {
            return new List<CustomerTypeSummaryDto>(); // ✅ Return an empty list instead of null
        }

        return bills
            .GroupBy(b => b.Customer?.CustomerType ?? "Unknown") // ✅ Handle potential null customers
            .Select(g => new CustomerTypeSummaryDto
            {
                CustomerType = g.Key,
                TotalRevenue = g.Sum(b => b.Total),
                TransactionCount = g.Count()
            })
            .ToList();
    }

    public async Task UpdateReportAsync(DailyReport report)
    {
        _context.Entry(report).State = EntityState.Modified; // ✅ Mark report as modified
        await _context.SaveChangesAsync(); // ✅ Save changes to the database
    }

    public async Task<List<CategorySummaryDto>> GetCategoryBreakdownAsync(DateTime date)
{
    var shiftRecord = await _context.DailyReports
        .Where(dr => dr.ReportDate.Date == date.Date)
        .OrderByDescending(dr => dr.ShiftStartTime)
        .AsNoTracking()
        .FirstOrDefaultAsync();

    if (shiftRecord == null)
        return new List<CategorySummaryDto>();

    var orders = await _context.Orders
        .Where(o => o.OrderDate >= shiftRecord.ShiftStartTime
            && (shiftRecord.ShiftEndTime == null || o.OrderDate <= shiftRecord.ShiftEndTime)
            && (o.Status == "Closed" || o.Status == "Submitted"))
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Article)
        .AsNoTracking()
        .ToListAsync();

    if (!orders.Any())
        return new List<CategorySummaryDto>();

    return orders
        .SelectMany(o => o.OrderItems)
        .Where(oi => oi.Article != null)
        .GroupBy(oi => oi.Article.Category)
        .Select(g => new CategorySummaryDto
        {
            Category = g.Key,
            TotalIncome = g.Sum(oi => oi.Quantity * oi.Price),
            TotalOrders = g.Count()
        })
        .ToList();
}


    public async Task SaveAsync(Shift shift)
    {
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();  // ✅ Asynchronous save method
    }

    public async Task<List<RestaurantTable>> GetOpenTablesForEmployeeAsync(int employeeId)
    {
        return await _context.RestaurantTables
            .Where(t => t.EmployeeId == employeeId && t.Status == true) // True indicates open table
            .ToListAsync();
    }

    public async Task<List<Orders>> GetClosedOrdersDuringShiftAsync(int employeeId, DateTime start, DateTime end)
    {
        return await _context.Orders
            .Where(o => o.EmployeeId == employeeId
                && o.OrderDate >= start
                && o.OrderDate <= end
                && o.Status == "Closed")
            .Include(o => o.Bills)
            .ToListAsync();
    }

    public async Task SaveReportAsync(DailyReport report)
    {
        _context.DailyReports.Add(report);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Shift shift)
    {
        _context.Shifts.Update(shift); 
        await _context.SaveChangesAsync(); 
    }

    // Get all daily reports between two dates
    public async Task<List<DailyReport>> GetDailyReportsInRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.DailyReports
            .Where(dr => dr.ReportDate >= startDate.Date && dr.ReportDate <= endDate.Date)
            .ToListAsync();
    }

    // Get most sold articles between two dates
  public async Task<List<MostSoldArticlesDto>> GetMostSoldArticlesInRangeAsync(DateTime startDate, DateTime endDate)
    {
        var items = await _context.OrderItems
            .Include(oi => oi.Article)
            .Include(oi => oi.Order)
                .ThenInclude(o => o.Bills)
            .Where(oi => oi.Order.Bills.Any(b =>
                b.IssueDate >= startDate &&
                b.IssueDate < endDate.AddDays(1)))
            .Select(oi => new
            {
                ArticleName = oi.Article.Name,
                oi.Quantity,
                oi.Price
            })
            .ToListAsync();

        var result = items
            .GroupBy(x => x.ArticleName)
            .Select(g => new MostSoldArticlesDto
            {
                ArticleName = g.Key,
                QuantitySold = g.Sum(x => x.Quantity),
                TotalIncome = g.Sum(x => x.Quantity * x.Price)
            })
            .OrderByDescending(dto => dto.QuantitySold)
            .Take(10)
            .ToList();

        return result;
    }





    // Get customer type breakdown between two dates
    public async Task<List<CustomerTypeSummaryDto>> GetCustomerTypeBreakdownInRangeAsync(DateTime startDate, DateTime endDate)
    {
        var bills = await _context.Bills
            .Where(b => b.IssueDate >= startDate && b.IssueDate < endDate.AddDays(1))
            .Include(b => b.Customer)
            .AsNoTracking()
            .ToListAsync();

        if (!bills.Any())
            return new List<CustomerTypeSummaryDto>();

        return bills
            .GroupBy(b => string.IsNullOrEmpty(b.Customer?.CustomerType) ? "Final Customer" : b.Customer.CustomerType)
            .Select(g => new CustomerTypeSummaryDto
            {
                CustomerType = g.Key,
                TotalRevenue = g.Sum(b => b.Total),
                TransactionCount = g.Count()
            })
            .ToList();
    }
}