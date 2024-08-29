using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Onyx.API.Products
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsDbContext _context;

        public ProductsController(ProductsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Products);
        }

        [HttpGet("{filterProperty}/{filterValue}")]
        public IActionResult GetProducts(string filterProperty, string filterValue)
        {
            //lambda can only compare a string property for equality with filterValue
            if(!LambdaFactory<Product>.TryCreateFilter(filterProperty, filterValue, out var lambda)) return BadRequest("Invalid filter for Product");  
            var filteredEntities = _context.Products.Where(lambda);
            return Ok(filteredEntities);
        }

        [HttpPut]
        public async Task<IActionResult> Put([Bind("Name, Colour")] Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException) {
                return Conflict("Could not add Product. Check if Product with same Name already exists.");
            }
            catch (Exception)
            {
                return Problem("Could not add Product.", statusCode: 500);
            }
            return Created();
        }
    }
}
