using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RestaurantAPI.DTOs;
using RestaurantAPI.Models;
using RestaurantAPI.Repositories;

namespace RestaurantAPI.Services;

public class ShiftService
{
    private readonly ShiftRepository _shiftRepository;
    private readonly IMapper _mapper;

    public ShiftService(ShiftRepository shiftRepository, IMapper mapper)
    {
        _shiftRepository = shiftRepository;
        _mapper = mapper;
    }

    public Shift? GetActiveShift(int employeeId)
    {
        return _shiftRepository.GetActiveShift(employeeId);
    }

    public async Task<ShiftDto> StartShiftAsync(StartShiftDto shiftDto)
    {
        var activeShift = _shiftRepository.GetActiveShift(shiftDto.UserId);
        if (activeShift != null)
            throw new InvalidOperationException("Cannot start a new shift before ending the previous one.");

        var newShift = new Shift
        {
            EmployeeId = shiftDto.UserId,
            StartTime = DateTime.UtcNow,
            EndTime = null
        };

        await _shiftRepository.SaveAsync(newShift);
        return _mapper.Map<ShiftDto>(newShift);
    }

    public void EndShift(int employeeId)
    {
        var activeShift = _shiftRepository.GetActiveShift(employeeId);
        if (activeShift == null)
            throw new InvalidOperationException("No active shift found.");

        activeShift.EndTime = DateTime.Now;
        _shiftRepository.Update(activeShift);
    }

    public List<Shift> GetShiftHistory(int employeeId)
    {
        return _shiftRepository.GetShiftHistory(employeeId);
    }

    // SHIFT CLOSURE + REPORT GENERATION

    public async Task<EnhancedDailyReportDto?> EndShiftAndGenerateReportAsync(int employeeId)
    {
        var activeShift = _shiftRepository.GetActiveShift(employeeId);
        if (activeShift == null)
            throw new InvalidOperationException("No active shift found.");

        var openTables = await _shiftRepository.GetOpenTablesForEmployeeAsync(employeeId);
        if (openTables.Any())
            throw new InvalidOperationException("Cannot end shift: open tables exist.");

        // ✅ Close the shift
        activeShift.EndTime = DateTime.UtcNow;
        await _shiftRepository.UpdateAsync(activeShift);

        // ✅ Get closed orders during this shift
        var orders = await _shiftRepository.GetClosedOrdersDuringShiftAsync(
            employeeId,
            activeShift.StartTime,
            activeShift.EndTime.Value
        );

        var bills = orders
            .SelectMany(o => o.Bills)
            .Where(b => b != null)
            .ToList();

        // ✅ Aggregate totals
        var totalIncome = bills.Sum(b => b.Total);
        var totalOrders = orders.Count;
        var totalPax = orders.Sum(o => o.PaxAmount);
        var totalDiscount = bills.Sum(b => b.Discount ?? 0);   // 👈 NEW

        // ✅ Save daily report record
        var report = new DailyReport
        {
            ReportDate = activeShift.EndTime.Value.Date,
            TotalIncome = totalIncome,
            TotalOrders = totalOrders,
            TotalPax = totalPax,
            ShiftStartUserId = activeShift.EmployeeId,
            ShiftEndUserId = employeeId,
            ShiftStatus = "Closed",
            ShiftStartTime = activeShift.StartTime,
            ShiftEndTime = activeShift.EndTime,
            TotalDiscount = totalDiscount
        };

        await _shiftRepository.SaveReportAsync(report);

        // ✅ Return enhanced DTO with discount included
        return new EnhancedDailyReportDto
        {
            ReportDate = report.ReportDate,
            TotalIncome = report.TotalIncome,
            TotalOrders = report.TotalOrders,
            TotalPax = report.TotalPax,
            TotalDiscount = report.TotalDiscount ?? 0,  
            CategoryBreakdown = await GetCategoryBreakdownAsync(report.ReportDate),
            PaymentBreakdown = await _shiftRepository.GetPaymentMethodBreakdownAsync(
                activeShift.StartTime,
                activeShift.EndTime.Value
            )
        };
    }

    // REPORT RETRIEVAL

    public async Task<EnhancedDailyReportDto?> GetDailyReportByIdAsync(int id)
{
    var report = await _shiftRepository.GetDailyReportByIdAsync(id);
    if (report == null) return null;

    if (report.ShiftEndTime == null)
    {
        return new EnhancedDailyReportDto
        {
            ReportDate = report.ReportDate,
            TotalIncome = report.TotalIncome,
            TotalOrders = report.TotalOrders,
            TotalPax = report.TotalPax,
            TotalDiscount = report.TotalDiscount ?? 0,
            CategoryBreakdown = await GetCategoryBreakdownAsync(report.ReportDate),
            PaymentBreakdown = new List<PaymentMethodSummaryDto>() // empty fallback
        };
    }

    return new EnhancedDailyReportDto
    {
        ReportDate = report.ReportDate,
        TotalIncome = report.TotalIncome,
        TotalOrders = report.TotalOrders,
        TotalPax = report.TotalPax,
        TotalDiscount = report.TotalDiscount ?? 0, 
        CategoryBreakdown = await GetCategoryBreakdownAsync(report.ReportDate),
        PaymentBreakdown = await _shiftRepository.GetPaymentMethodBreakdownAsync(
            report.ShiftStartTime,
            report.ShiftEndTime.Value
        )
    };
}


    public async Task<EnhancedDailyReportDto?> GetDailyReportByDateAsync(DateTime date)
    {
        var report = await _shiftRepository.GetDailyReportByDateAsync(date);

        if (report == null)
            return null;

        if (report.ShiftEndTime == null)
            return new EnhancedDailyReportDto
            {
                ReportDate = report.ReportDate,
                TotalIncome = report.TotalIncome,
                TotalOrders = report.TotalOrders,
                TotalPax = report.TotalPax,
                TotalDiscount = report.TotalDiscount ?? 0,
                CategoryBreakdown = await GetCategoryBreakdownAsync(report.ReportDate),
                PaymentBreakdown = new List<PaymentMethodSummaryDto>() // empty list fallback
            };

        return new EnhancedDailyReportDto
        {
            ReportDate = report.ReportDate,
            TotalIncome = report.TotalIncome,
            TotalOrders = report.TotalOrders,
            TotalPax = report.TotalPax,
            TotalDiscount = report.TotalDiscount ?? 0,
            CategoryBreakdown = await GetCategoryBreakdownAsync(report.ReportDate),
            PaymentBreakdown = await _shiftRepository.GetPaymentMethodBreakdownAsync(
                report.ShiftStartTime,
                report.ShiftEndTime.Value
            )
        };
    }

    public async Task<DailyReport?> GetCurrentShiftAsync()
    {
        return await _shiftRepository.GetCurrentShiftAsync();
    }


    public async Task<CustomerTypeReportDto?> GetCustomerTypeBreakdownAsync(DateTime date)
    {
        var breakdown = await _shiftRepository.GetCustomerTypeBreakdownAsync(date);
        if (breakdown == null) return null;

        return new CustomerTypeReportDto
        {
            ReportDate = date,
            CustomerTypeBreakdown = breakdown
        };
    }

    private async Task<List<CategorySummaryDto>> GetCategoryBreakdownAsync(DateTime date)
    {
        return await _shiftRepository.GetCategoryBreakdownAsync(date);
    }

    public async Task<List<PaymentMethodSummaryDto>> GetPaymentMethodBreakdownAsync(DateTime start, DateTime end)
    {
        return await _shiftRepository.GetPaymentMethodBreakdownAsync(start, end);
    }

    // Sales report across a range of dates
    public async Task<SalesReportDto?> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var reports = await _shiftRepository.GetDailyReportsInRangeAsync(startDate, endDate);
        if (reports == null || !reports.Any()) return null;

        return new SalesReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = reports.Sum(r => r.TotalIncome ?? 0),
            TotalOrders = reports.Sum(r => r.TotalOrders ?? 0),
            TotalPax = reports.Sum(r => r.TotalPax ?? 0),
            TotalDiscount = reports.Sum(r => r.TotalDiscount ?? 0)
        };
    }

public async Task<List<MostSoldArticlesDto>> GetMostSoldArticlesAsync(DateTime startDate, DateTime endDate)
{
    var articles = await _shiftRepository.GetMostSoldArticlesInRangeAsync(startDate, endDate);

    return articles.Select(a => new MostSoldArticlesDto
    {
        ArticleName = a.ArticleName,
        QuantitySold = a.QuantitySold,
        TotalIncome = a.TotalIncome
    }).ToList(); // ✅ strips EF proxy
}

public async Task<CustomerTypeReportDto> GetCustomerTypeBreakdownAsync(DateTime startDate, DateTime endDate)
{
    var breakdown = await _shiftRepository.GetCustomerTypeBreakdownInRangeAsync(startDate, endDate)
                    ?? new List<CustomerTypeSummaryDto>();

    var cleanBreakdown = breakdown.Select(ct => new CustomerTypeSummaryDto
    {
        CustomerType = string.IsNullOrEmpty(ct.CustomerType) ? "Final Customer" : ct.CustomerType,
        TransactionCount = ct.TransactionCount,
        TotalRevenue = ct.TotalRevenue
    }).ToList();

    return new CustomerTypeReportDto
    {
        StartDate = startDate,
        EndDate = endDate,
        ReportDate = null,
        CustomerTypeBreakdown = cleanBreakdown
    };
}



}
