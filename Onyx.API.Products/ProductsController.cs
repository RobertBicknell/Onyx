using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
            //can only compare a string property for equality with filterValue
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
            catch (DbUpdateException exDbUpdate) {
                return BadRequest("Could not add Product. Check if Product with same Name already exists.");
            }
            catch (Exception e)
            {
                return Problem("Could not add Product.");
            }
            return Created();
        }
    }
}
