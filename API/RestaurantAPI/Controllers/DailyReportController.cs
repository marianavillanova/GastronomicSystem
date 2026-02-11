using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Services;
using RestaurantAPI.Models;
using RestaurantAPI.DTOs;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailyReportController : ControllerBase
    {
        private readonly ShiftService _shiftService;
        private readonly GastronomicSystemContext _context;
        public DailyReportController(ShiftService shiftService, GastronomicSystemContext context)
        {
            _shiftService = shiftService;
            _context = context;
        }

        // PUT End Shift
        [HttpPut("endShift")]
        public async Task<IActionResult> EndShift([FromBody] EndShiftDto shiftDto)
        {
            try
            {
                var result = await _shiftService.EndShiftAndGenerateReportAsync(shiftDto.UserId);

                if (result == null)
                    return NotFound("Daily Report not found.");

                return Ok(new { message = "Shift successfully closed.", result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/dailyreport/byId/{id}
        [HttpGet("byId/{id}")]
        public async Task<ActionResult<EnhancedDailyReportDto>> GetDailyReportById(int id)
        {
            var report = await _shiftService.GetDailyReportByIdAsync(id);

            if (report == null)
            {
                return NotFound("Daily Report not found.");
            }

            return Ok(report);
        }

        // GET api/dailyreport/{date}
        [HttpGet("{date}")]
        public async Task<ActionResult<EnhancedDailyReportDto>> GetDailyReportWithCategories(DateTime date)
        {
            var report = await _shiftService.GetDailyReportByDateAsync(date);
            
            if (report == null)
            {
                return NotFound($"No shift record found for {date.Date}");
            }

            return Ok(report);
        }

        // GET api/dailyreport/paymentmethods/{date}
        [HttpGet("paymentmethods/{date}")]
        public async Task<ActionResult<PaymentMethodReportDto>> GetPaymentMethodBreakdown(DateTime date)
        {
            // Define the range for the given date (whole day)
            var start = date.Date;
            var end = date.Date.AddDays(1).AddTicks(-1);

            var breakdown = await _shiftService.GetPaymentMethodBreakdownAsync(start, end);

            if (breakdown == null || !breakdown.Any())
            {
                return NotFound("No bills found for the given date.");
            }

            return Ok(new PaymentMethodReportDto
            {
                ReportDate = date,
                PaymentMethodBreakdown = breakdown
            });
        }


    // Sales report for a date range (aggregated)
    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(DateTime startDate, DateTime endDate)
    {
        var report = await _shiftService.GetSalesReportAsync(startDate, endDate);
        if (report == null) return NotFound("No sales data found");
        return Ok(report);
    }

    [HttpGet("mostsold")]
    public async Task<ActionResult<IEnumerable<MostSoldArticlesDto>>> GetMostSoldArticles(DateTime startDate, DateTime endDate)
    {
        var articles = await _shiftService.GetMostSoldArticlesAsync(startDate, endDate);

        // ✅ Force materialization into a plain list
        var clean = articles?.Select(a => new MostSoldArticlesDto
        {
            ArticleName = a.ArticleName,
            QuantitySold = a.QuantitySold,
            TotalIncome = a.TotalIncome
        }).ToList() ?? new List<MostSoldArticlesDto>();

        if (!clean.Any()) return Ok(new List<MostSoldArticlesDto>());
        return Ok(clean);
    }


    [HttpGet("customertypes")]
    public async Task<ActionResult<CustomerTypeReportDto>> GetCustomerTypes(DateTime startDate, DateTime endDate)
    {
        var report = await _shiftService.GetCustomerTypeBreakdownAsync(startDate, endDate);
        if (report == null) return NotFound("No customer type data found");

        // ✅ Normalize null customer types and force plain list
        var cleanBreakdown = report.CustomerTypeBreakdown
            ?.Select(ct => new CustomerTypeSummaryDto
            {
                CustomerType = string.IsNullOrEmpty(ct.CustomerType) ? "Final Customer" : ct.CustomerType,
                TransactionCount = ct.TransactionCount,
                TotalRevenue = ct.TotalRevenue
            }).ToList() ?? new List<CustomerTypeSummaryDto>();

        var cleanReport = new CustomerTypeReportDto
        {
            StartDate = report.StartDate,
            EndDate = report.EndDate,
            ReportDate = report.ReportDate,
            CustomerTypeBreakdown = cleanBreakdown
        };

        return Ok(cleanReport);
    }


    }
}