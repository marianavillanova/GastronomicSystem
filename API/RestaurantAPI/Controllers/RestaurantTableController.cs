using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantTableController : ControllerBase
    {
        private readonly GastronomicSystemContext _context;

        public RestaurantTableController(GastronomicSystemContext context)
        {
            _context = context;
        }

        // GET: api/restauranttable
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantTable>>> GetAllTables()
        {
            return await _context.RestaurantTables.ToListAsync();
        }

        // GET: api/restauranttable/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantTable>> GetTable(int id)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null)
            {
                return NotFound(new { ErrorCode = "TABLE_NOT_FOUND", Message = "Table not found." });
            }

            return Ok(table);
        }

        // PUT: api/restauranttable/{id}/prepare
        [HttpPut("{id}/prepare")]
        public async Task<IActionResult> PrepareTable(int id, [FromBody] OpenTableDto request)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null)
            {
                return NotFound(new { ErrorCode = "TABLE_NOT_FOUND", Message = "Table not found." });
            }

            table.EmployeeId = request.EmployeeId;
            table.Pax = request.PaxAmount; // ✅ Add this line

            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(table);
        }


        // PUT: api/restauranttable/{id}/updateStatus
        [HttpPut("{id}/updateStatus")]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] TableStatusUpdateDto request)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null)
            {
                return NotFound(new { ErrorCode = "TABLE_NOT_FOUND", Message = "Table not found." });
            }

            // Assign EmployeeId when occupied, remove when free
            table.Status = request.Status;
            table.EmployeeId = request.Status ? request.EmployeeId : null;

            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/restauranttable/statuses
        [HttpGet("statuses")]
        public async Task<ActionResult<IEnumerable<object>>> GetTableStatuses()
        {
            var tables = await _context.RestaurantTables
                .Select(t => new
                {
                    TableId = t.TableId,
                    IsAvailable = !t.Status,
                    Pax = t.Pax,
                    EmployeeId = !t.Status ? t.EmployeeId : null  // ✅ Show EmployeeId only if occupied
                })
                .ToListAsync();

            if (!tables.Any())
            {
                return NotFound(new { ErrorCode = "NO_TABLES_FOUND", Message = "No tables found." });
            }

            return Ok(tables);
        }

        // PUT: api/restauranttable/{id}/updatePax
        [HttpPut("{id}/updatePax")]
        public async Task<IActionResult> UpdateTablePax(int id, [FromBody] int pax)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null)
            {
                return NotFound(new { ErrorCode = "TABLE_NOT_FOUND", Message = "Table not found." });
            }

            table.Pax = pax;
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/restauranttable/{id}/open
        [HttpPut("{id}/open")]
        public async Task<IActionResult> OpenTable(int id, [FromBody] OpenTableDto request)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null)
            {
                return NotFound(new { ErrorCode = "TABLE_NOT_FOUND", Message = "Table not found." });
            }

            if (table.Status)
            {
                // ✅ Table is already open — return success instead of error
                return Ok(table);
            }


            table.Status = true;
            table.EmployeeId = request.EmployeeId;
            table.Pax = request.PaxAmount;

            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(table);
        }


    }
}