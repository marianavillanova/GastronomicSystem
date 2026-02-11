using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using RestaurantAPI.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


[ApiController]
[Route("api/[controller]")]
public class ShiftController : ControllerBase
{
    private readonly ShiftService _shiftService;
    

    public ShiftController(ShiftService shiftService)
    {
        _shiftService = shiftService;
    }


    // ✅ Get Active Shift
    [HttpGet("active")]
    public IActionResult GetActiveShift(int employeeId)
    {
        try
        {
            var activeShift = _shiftService.GetActiveShift(employeeId);
            return Ok(activeShift);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    //current shift for guard
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentShift()
    {
        var shift = await _shiftService.GetCurrentShiftAsync();
        return Ok(shift);
    }





    // ✅ Start a Shift
    [HttpPost("start")]
    public async Task<IActionResult> StartShift([FromBody] StartShiftDto shiftDto)
    {
        try
        {
            var shiftDtoResult = await _shiftService.StartShiftAsync(shiftDto);
            return Ok(shiftDtoResult);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


        // End a Shift and Generate Report
        [HttpPost("end")]
        public async Task<IActionResult> EndShift([FromBody] EndShiftDto dto)
        {
            var report = await _shiftService.EndShiftAndGenerateReportAsync(dto.UserId);
            if (report == null) return BadRequest("No report generated.");

            // Return JSON so the frontend can refresh UI state
            return Ok(report);
        }

        [HttpGet("report/pdf/{date}")]
        public async Task<IActionResult> GetReportPdf(DateTime date)
        {
            var report = await _shiftService.GetDailyReportByDateAsync(date);
            if (report == null) return NotFound();

            var pdfBytes = GenerateReportPdf(report);
            return File(pdfBytes, "application/pdf", $"DailyReport_{report.ReportDate:yyyyMMdd}.pdf");
        }



        private byte[] GenerateReportPdf(EnhancedDailyReportDto report)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    page.Header().Text($"Daily Report - {report.ReportDate:dd/MM/yyyy}")
                        .FontSize(20).Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Total Income: {report.TotalIncome}");
                        col.Item().Text($"Total Discount: {report.TotalDiscount}");
                        col.Item().Text($"Total Orders: {report.TotalOrders}");
                        col.Item().Text($"Total Pax: {report.TotalPax}");

                        // ✅ Category Breakdown
                        if (report.CategoryBreakdown.Any())
                        {
                            col.Item().Text("Category Breakdown:").Bold();
                            foreach (var category in report.CategoryBreakdown)
                            {
                                var income = category.TotalIncome ?? 0m;
                                col.Item().Text($"{category.Category}: $ {income} (Total Sold items: {category.TotalOrders})");
                            }
                        }

                        // ✅ Payment Method Breakdown
                            if (report.PaymentBreakdown.Any())
                            {
                                col.Item().Text("Payment Method Breakdown:").Bold();
                                foreach (var payment in report.PaymentBreakdown)
                                {
                                    col.Item().Text($"{payment.PaymentMethod}: {payment.TotalRevenue:C} (Transactions: {payment.TransactionCount})");
                                }
                            }
                    });

                    page.Footer().AlignCenter().Text("Generated with QuestPDF");
                });
            });

            return document.GeneratePdf();
        }


}
