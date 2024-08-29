using Microsoft.Extensions.Configuration;
using Onyx.API.Products;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetValue<string>("ConnectionString");
var applicationUrl = builder.Configuration["ApplicationUrl"];
var identityServer = builder.Configuration["IdentityServer"];

builder.WebHost.UseUrls(applicationUrl);
builder.Services.AddControllers();
builder.Services.AddDbContext<IProductsDbContext, ProductsDbContext>(
    options => options.UseSqlServer(connectionString)); 
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = identityServer; 
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
builder.Services.AddHealthChecks()
    .AddCheck(
        "ProductsDB-check",
        new SqlConnectionHealthCheck(connectionString),
        HealthStatus.Unhealthy,
        new string[] { "productsdb" });



var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("identity", (ClaimsPrincipal user) => user.Claims.Select(c => new { c.Type, c.Value }))
    .RequireAuthorization("ApiScope");
app.MapHealthChecks("/hc");

app.Run();
