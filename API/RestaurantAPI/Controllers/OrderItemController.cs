using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.DTOs;
using RestaurantAPI.Models;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly GastronomicSystemContext _context;

        public OrderItemController(GastronomicSystemContext context)
        {
            _context = context;
        }

        // ✅ POST: api/orderitem (Add Order Item)
        [HttpPost]
        public async Task<ActionResult<OrderItem>> AddOrderItem([FromBody] CreateOrderItemDto dto)
        {
            // ✅ Check for an existing active order for this table
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.TableId == dto.TableId && (o.Status == "Pending" || o.Status == "Submitted"));

            var article = await _context.Articles.FindAsync(dto.ArticleId);

            if (article == null)
            {
                return NotFound(new {
                    ErrorCode = "ARTICLE_NOT_FOUND",
                    Message = "Article not found."
                });
            }

            // ✅ If no active order exists, create a new one
            if (order == null)
            {
                var table = await _context.RestaurantTables.FindAsync(dto.TableId);

                if (table == null || table.Status == false)
                {
                    return BadRequest(new {
                        ErrorCode = "TABLE_NOT_OPEN",
                        Message = "Cannot create a new order — table is not open."
                    });
                }

                // ✅ Safely extract EmployeeId using pattern matching
                if (table.EmployeeId is not int employeeId)
                {
                    return BadRequest(new {
                        ErrorCode = "EMPLOYEE_NOT_SET",
                        Message = "Cannot create order — employee not assigned to table."
                    });
                }

                // ✅ Create new order
                order = new Orders
                {
                    TableId = dto.TableId,
                    EmployeeId = employeeId,
                    PaxAmount = table.Pax ?? 1,
                    OrderDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                Console.WriteLine($"🆕 Created new order {order.OrderId} for table {dto.TableId} with status: {order.Status}");
            }
            else
            {
                Console.WriteLine($"🔁 Reusing existing order {order.OrderId} for table {dto.TableId} with status: {order.Status}");
            }

            // ✅ Construct the OrderItem manually
            var orderItem = new OrderItem
            {
                TableId = dto.TableId,
                ArticleId = dto.ArticleId,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Comment = dto.Comment,
                OrderId = order.OrderId,
                Discount = dto.Discount
            };

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            Console.WriteLine($"➕ Added item {dto.ArticleId} to order {order.OrderId}");

            return CreatedAtAction(nameof(GetOrderItems), new { orderId = orderItem.OrderId }, orderItem);
        }



        // ✅ GET: api/orderitem/order/{orderId} (Retrieve Order Items)
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrderItems(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(new { ErrorCode = "ORDER_NOT_FOUND", Message = "Order not found." });
            }

            var table = await _context.RestaurantTables.FindAsync(order.TableId);

            var items = await _context.OrderItems
                .Include(oi => oi.Order)  // ✅ Load related Order details
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new
                {
                    OrderItemId = oi.OrderItemId,
                    ArticleId = oi.ArticleId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    TableId = oi.TableId,
                    EmployeeId = oi.Order.EmployeeId,
                    Comment = oi.Comment
                })
                .ToListAsync();

            return Ok(items);
        }

        // ✅ PUT: api/orderitem/{id}/updateQuantity (Update Order Item Quantity)
        [HttpPut("{id}/updateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int quantity)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound(new { ErrorCode = "ORDER_ITEM_NOT_FOUND", Message = "Order item not found." });
            }

            var order = await _context.Orders.FindAsync(orderItem.OrderId);
            var article = await _context.Articles.FindAsync(orderItem.ArticleId);

            if (order == null || article == null)
            {
                return NotFound(new { ErrorCode = "NOT_FOUND", Message = "Order or article not found." });
            }

            if (order.Status == "Submitted")
            {
                return BadRequest(new { ErrorCode = "ORDER_SUBMITTED", Message = "Cannot update items in a submitted order." });
            }

            if (quantity <= 0)
            {
                return BadRequest(new { ErrorCode = "INVALID_QUANTITY", Message = "Quantity must be greater than zero." });
            }

            // ✅ Update the quantity and price
            orderItem.Quantity = quantity;
            orderItem.Price = article.Price * quantity;

            _context.Entry(orderItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/orderitem/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { ErrorCode = "ITEM_NOT_FOUND", Message = "Order item not found." });
            }

            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();

            Console.WriteLine($"🗑️ Deleted order item {id}");
            return NoContent();
        }

    }
}