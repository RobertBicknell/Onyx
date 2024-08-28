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
            if(!LambdaFactory<Product>.TryCreateFilter(filterProperty, filterValue, out var lambda)) return BadRequest();  
            var filteredEntities = _context.Products.Where(lambda);
            return Ok(filteredEntities);
        }

        [HttpPut]
        public async Task<IActionResult> Put([Bind("Name, Colour")] Product productModel)
        {
            //TODO review for error / injection
            await _context.Products.AddAsync(productModel);
            await _context.SaveChangesAsync();
            return Created();
        }
    }
}
