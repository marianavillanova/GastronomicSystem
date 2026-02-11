namespace RestaurantAPI.DTOs;

// DTO for Customer Type Report
        public class CustomerTypeReportDto
        {
            public DateTime? ReportDate { get; set; }  
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<CustomerTypeSummaryDto> CustomerTypeBreakdown { get; set; } = new();
            }
