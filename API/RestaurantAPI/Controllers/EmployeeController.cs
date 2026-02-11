using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly GastronomicSystemContext _context;

        public EmployeeController(GastronomicSystemContext context)
        {
            _context = context;
        }

        // POST: api/employee/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] string loginCode)
        {
            Console.WriteLine($"🔐 Received login code: {loginCode}");


            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.LoginCode == loginCode);

            if (employee == null)
            {
                return Unauthorized("Invalid login code");
            }

            // Include role information in the response
            var response = new
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                Role = employee.Role
            };

            return Ok(response);
        }

        // GET: api/employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAllEmployees()
        {
            return await _context.Employees.ToListAsync();
        }
    }
}