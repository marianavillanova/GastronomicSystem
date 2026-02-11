using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs;
using RestaurantAPI.Models;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly GastronomicSystemContext _context;

        public OrderController(GastronomicSystemContext context)
        {
            _context = context;
        }

        // POST: api/order/create
        [HttpPost("create")]
        public async Task<ActionResult<Orders>> CreateOrder([FromBody] CreateOrderDto request)
        {
            // ✅ Validate table existence
            var table = await _context.RestaurantTables.FindAsync(request.TableId);
            if (table == null)
            {
                return NotFound(new { ErrorCode = "TABLE_NOT_FOUND", Message = "Table not found." });
            }

            // ✅ Check for existing active orders
            var existingOrder = await _context.Orders
                .Where(o => o.TableId == request.TableId && (o.Status == "Pending" || o.Status == "Submitted"))
                .FirstOrDefaultAsync();

            if (table.Status && existingOrder == null)
            {
                return BadRequest(new { ErrorCode = "TABLE_OCCUPIED", Message = "This table is currently occupied." });
            }


            if (existingOrder != null)
            {
                return Ok(existingOrder); // ✅ Return the existing active order
            }

            // ✅ Extract and validate `EmployeeId` directly from the table
            if (table.EmployeeId == null)
            {
                return BadRequest(new { ErrorCode = "EMPLOYEE_NOT_FOUND", Message = "No employee assigned to this table." });
            }

            var employeeId = table.EmployeeId.Value;

            // ✅ Create new order
            var newOrder = new Orders
            {
                TableId = request.TableId,
                EmployeeId = employeeId,
                PaxAmount = table.Pax ?? 1,
                OrderDate = DateTime.Now,
                Status = "Pending",
                GlobalDiscount = request.GlobalDiscount
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();
            Console.WriteLine($"🆕 Created order {newOrder.OrderId} with status: {newOrder.Status}");

            // ✅ Update table status
            table.Status = true;
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // ✅ Retrieve full order with employee
            var createdOrder = await _context.Orders
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(o => o.OrderId == newOrder.OrderId);

            if (createdOrder == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order was not found after creation." });
            }

            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.OrderId }, createdOrder);
        }


        // GET: api/order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Orders>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)  // ✅ Ensure order items load
                .Include(o => o.Table)       // ✅ Include restaurant table info
                .Include(o => o.Employee)    // ✅ Load assigned Employee details
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order not found." });
            }

            if (order.Status == "Billed")
            {
                return BadRequest(new { ErrorCode = "ORDER_BILLED", Message = "This order has already been billed." });
            }

            return Ok(order);
        }

        /// GET: api/order/table/{tableId}
        [HttpGet("table/{tableId}")]
        public async Task<ActionResult<Orders>> GetActiveOrderForTable(int tableId)
        {
            var table = await _context.RestaurantTables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound(new { ErrorCode = "TABLE_NOT_FOUND", Message = "Table not found." });
            }

            var activeOrder = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi=>oi.Article)
                .Include(o => o.Employee)
                .Include(o => o.Table)
                .Where(o => o.TableId == tableId && (o.Status == "Pending" || o.Status == "Submitted"))
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            if (activeOrder == null)
            {
                return Ok(null); 
            }


            return Ok(activeOrder);
        }


        // PUT: api/order/{id}/submit
        [HttpPut("{id}/submit")]
        public async Task<IActionResult> SubmitOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order not found." });
            }

            if (!order.OrderItems.Any())
            {
                return BadRequest(new { ErrorCode = "EMPTY_ORDER", Message = "Cannot submit an order with no items." });
            }

            Console.WriteLine($"📤 Submitting order {order.OrderId} with {order.OrderItems.Count} items");

            order.Status = "Submitted";
            _context.Entry(order).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            Console.WriteLine($"Order {id} has been submitted and billed.");

            return NoContent();
        }

        // PUT: api/order/{orderId}/assign-customer
        [HttpPut("{orderId}/assign-customer")]
        public async Task<IActionResult> AssignCustomerToOrder(int orderId, [FromBody] int customerId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order not found." });
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound(new { ErrorCode = "CUSTOMER_NOT_FOUND", Message = "Customer not found." });
            }

            order.CustomerId = customerId;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/order/{id}/discount
        [HttpPut("{id}/discount")]
        public async Task<IActionResult> SetGlobalDiscount(int id, [FromBody] decimal discount)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order not found." });
            }

            order.GlobalDiscount = discount;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            Console.WriteLine($"🌍 Global discount {discount}% applied to order {id}");

            return NoContent();
        }

        // PUT: api/order/{orderId}/closeTable
        [HttpPut("/api/order/{orderId}/closeTable")]
        public async Task<IActionResult> CloseTableByOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order not found." });
            }

            var table = await _context.RestaurantTables.FindAsync(order.TableId);
            if (table != null)
            {
                table.Status = false;
                table.EmployeeId = null;
                table.Pax = null;
                _context.Entry(table).State = EntityState.Modified;
            }

            order.Status = "Closed";
            _context.Entry(order).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            Console.WriteLine($"🧹 Table {table?.TableId} reset — order {order.OrderId} marked as Closed");

            return Ok(new { Message = "Table closed successfully.", TableId = table?.TableId });
        }

    }
}