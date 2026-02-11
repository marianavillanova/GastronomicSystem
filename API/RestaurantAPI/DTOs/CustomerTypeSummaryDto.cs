namespace RestaurantAPI.DTOs;

// DTO for Customer Type Breakdown
        public class CustomerTypeSummaryDto
        {
            public string CustomerType { get; set; } = null!; // "Corporate" or "Individual"
            public decimal TotalRevenue { get; set; } // Total revenue from this customer type
            public int TransactionCount { get; set; } // Number of transactions from this customer type
        }