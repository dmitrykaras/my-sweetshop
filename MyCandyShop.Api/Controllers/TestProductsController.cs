using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Data;

namespace MySweetShop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/testproducts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    p.CategoryId
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/testproducts/byid/{id}
        [HttpGet("byid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    p.CategoryId
                })
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();

            return Ok(product);
        }

        // GET: api/testproducts/byname?name=Наполеон
        [HttpGet("byname")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            var products = await _context.Products
                .Where(p => p.Name.Contains(name))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    p.CategoryId
                })
                .ToListAsync();

            return Ok(products);
        }
    }
}
