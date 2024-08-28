using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Onyx.API.Products
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options): base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

            //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Products;ConnectRetryCount=0");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()

                .HasIndex(e => e.Name)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        
        }

        public DbSet<Product> Products { get; set; }
    }
}
