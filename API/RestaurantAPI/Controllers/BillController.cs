using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;
using RestaurantAPI.DTOs;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillController : ControllerBase
    {
        private readonly GastronomicSystemContext _context;

        public BillController(GastronomicSystemContext context)
        {
            _context = context;
        }

        // POST: api/bill
        [HttpPost]
        public async Task<ActionResult<Bill>> CreateBill([FromBody] CreateBillDto request)
        {
            int orderId = request.OrderId;

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order not found." });
            }

            if (order.Status != "Submitted")
            {
                return BadRequest(new { ErrorCode = "ORDER_NOT_SUBMITTED", Message = "Order must be submitted before generating a bill." });
            }

            var existingBill = await _context.Bills.FirstOrDefaultAsync(b => b.OrderId == orderId);
            if (existingBill != null)
            {
                return BadRequest(new { ErrorCode = "BILL_EXISTS", Message = "A bill has already been generated for this order." });
            }

            var subtotal = request.Subtotal ?? order.OrderItems.Sum(item => item.Price);
            var discount = request.Discount ?? 0m;
            var total = subtotal - discount;

            var newBill = new Bill
            {
                OrderId = request.OrderId,
                Subtotal = subtotal,
                Discount = discount,
                Total = total,
                IssueDate = DateTime.Now,
                PaymentMethod = request.PaymentMethod,
                SplitCashAmount = request.PaymentMethod == "split" ? request.SplitCashAmount : null,
                SplitCardAmount = request.PaymentMethod == "split" ? request.SplitCardAmount : null,
                CustomerId = order.CustomerId,
            };


            _context.Bills.Add(newBill);
            order.Status = "Billed";
            _context.Entry(order).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBill), new { id = newBill.BillId }, newBill);
        }

        // GET: api/bill
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillDto>>> GetAllBills()
        {
            var bills = await _context.Bills
                .Include(b => b.Order)
                .Select(b => new BillDto
                {
                    BillId = b.BillId,
                    OrderId = b.OrderId,
                    Total = b.Total,
                    IssueDate = b.IssueDate,
                    Discount = b.Discount,
                    PaymentMethod = b.PaymentMethod,
                    CustomerId = b.CustomerId,
                    Subtotal = b.Subtotal
                })
                .ToListAsync();
            return Ok(bills);
        }

        // GET: api/bill/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Bill>> GetBill(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Order)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (bill == null)
            {
                return NotFound(new { ErrorCode = "BILL_NOT_FOUND", Message = "Bill not found." });
            }

            return Ok(bill);
        }

        // GET: api/bill/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<Bill>>> GetBillsByDate(DateTime date)
        {
            var bills = await _context.Bills
                .Where(b => b.IssueDate.Date == date.Date)
                .ToListAsync();

            if (!bills.Any())
            {
                return NotFound(new { ErrorCode = "NO_BILLS_FOUND", Message = "No bills found for the given date." });
            }

            return Ok(bills);
        }

        // PUT: api/bill/{id}/pay
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> RecordPayment(int id, [FromBody] string paymentMethod)
        {
            var bill = await _context.Bills.FindAsync(id);

            if (bill == null)
            {
                return NotFound(new { ErrorCode = "BILL_NOT_FOUND", Message = "Bill not found." });
            }

            bill.PaymentMethod = paymentMethod;
            _context.Entry(bill).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

       // PUT: api/bill/{id}/processPayment
        [HttpPut("{id}/processPayment")]
        public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentRequest payment)
        {
            var bill = await _context.Bills.FindAsync(id);

            if (bill == null)
            {
                return NotFound(new { ErrorCode = "BILL_NOT_FOUND", Message = "Bill not found." });
            }

            // Validate payment total
            if (payment.TotalPaid < bill.Total)
            {
                return BadRequest(new { ErrorCode = "INSUFFICIENT_PAYMENT", Message = "Payment does not cover the total bill amount." });
            }

            // Handle customer type
            if (payment.CustomerType == "Corporate")
            {
                if (payment.CompanyInfo == null || string.IsNullOrEmpty(payment.CompanyInfo.CompanyName) || string.IsNullOrEmpty(payment.CompanyInfo.TaxNumber))
                {
                    return BadRequest(new { ErrorCode = "MISSING_COMPANY_INFO", Message = "Company name and tax number are required for corporate customers." });
                }

                // Validate corporate customer
                var customer = await _context.Customers.FindAsync(payment.CompanyInfo.CustomerId);
                if (customer == null || customer.CustomerType != "Corporate")
                {
                    return BadRequest(new { ErrorCode = "INVALID_CUSTOMER", Message = "The customer is not a valid corporate customer." });
                }

                // Associate bill with corporate customer
                bill.CustomerId = payment.CompanyInfo.CustomerId;
            }
            else if (payment.CustomerType == "Final")
            {
                // No additional details required for final customer
                bill.CustomerId = null; // Reset CustomerId (optional step)
            }
            else
            {
                return BadRequest(new { ErrorCode = "INVALID_CUSTOMER_TYPE", Message = "Customer type must be either 'Corporate' or 'Final'." });
            }

        // Record payment method(s)
        bill.PaymentMethod = string.Join(", ", payment.Methods);

        _context.Entry(bill).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Payment processed successfully.", BillId = bill.BillId });
    }


    }
}