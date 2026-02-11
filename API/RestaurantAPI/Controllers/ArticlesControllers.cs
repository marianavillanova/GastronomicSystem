using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly GastronomicSystemContext _context;

        public ArticlesController(GastronomicSystemContext context)
        {
            _context = context;
        }

        // GET: api/articles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetMenu()
        {
            return await _context.Articles.ToListAsync();
        }

        // GET: api/articles/category/{category}
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticlesByCategory(string category)
        {
            return await _context.Articles.Where(a => a.Category == category).ToListAsync();
        }


        // PUT: api/articles/{id}/updatePrice
        [HttpPut("{id}/updatePrice")]
        public async Task<IActionResult> UpdatePrice(int id, decimal newPrice)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound(new { ErrorCode = "ARTICLE_NOT_FOUND", Message = "Article not found." });
            }

            article.Price = newPrice;
            _context.Entry(article).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/articles/{id}/disable
        [HttpPut("{id}/disable")]
        public async Task<IActionResult> DisableArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);

            if (article == null)
            {
                return NotFound(new { ErrorCode = "ARTICLE_NOT_FOUND", Message = "Article not found." });
            }

            if (!article.IsActive)
            {
                return BadRequest(new { ErrorCode = "ARTICLE_ALREADY_DISABLED", Message = "This article is already disabled." });
            }

            article.IsActive = false;
            _context.Entry(article).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Article disabled successfully.", ArticleId = article.ArticleId });
        }



        // POST: api/articles/add
        [HttpPost("add")]
        public async Task<ActionResult<Article>> AddArticle([FromBody] Article article)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(article.Name) || article.Price <= 0)
            {
                return BadRequest(new { ErrorCode = "INVALID_ARTICLE", Message = "Article name and price are required and must be valid." });
            }

            // Check if an article with the same name already exists
            var existingArticle = await _context.Articles.FirstOrDefaultAsync(a => a.Name == article.Name);
            if (existingArticle != null)
            {
                return Conflict(new { ErrorCode = "ARTICLE_EXISTS", Message = "An article with the same name already exists." });
            }

            // Add the new article to the database
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenu), new { id = article.ArticleId }, article);
        }
    }
}