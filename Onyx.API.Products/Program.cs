using Microsoft.Extensions.Configuration;
using Onyx.API.Products;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ProductsDbContext>(
//options => options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Products;ConnectRetryCount=0"));
    options => options.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=Products;Integrated Security=True;TrustServerCertificate=True;ConnectRetryCount=0"));

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001"; //todo can be loaded from config
        options.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "products");
    });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

//dbContext.Database.EnsureCreated();
//try
//{
//    dbContext.Database.Migrate();
//}
//catch (Exception) { }



//LambdaMapper<Product, string>.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("identity", (ClaimsPrincipal user) => user.Claims.Select(c => new { c.Type, c.Value }))
    .RequireAuthorization("ApiScope");

app.Run();
