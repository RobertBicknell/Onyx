using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Onyx.API.Products
{
    public class ProductsDbContext : DbContext, IProductsDbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options): base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //DEBUG - verifies lamba filter for "colour" in ProductController/GET translates to SQL where clause
            //optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information); 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()

                .HasIndex(e => e.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(e => e.Colour);

            base.OnModelCreating(modelBuilder);
        
        }

        public DbSet<Product> Products { get; set; }
    }

    internal interface IProductsDbContext
    {
        public DbSet<Product> Products { get; set; }
    }
}
