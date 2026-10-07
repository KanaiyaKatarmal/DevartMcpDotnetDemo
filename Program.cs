using DevartMcpDotnetDemo.Data;
using DevartMcpDotnetDemo.Services;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Devart MCP PostgreSQL Demo API",
        Version = "v1",
        Description = "ASP.NET Core Web API on .NET 10 using Devart dotConnect for PostgreSQL EF Core."
    });
});

// Configure Devart dotConnect for PostgreSQL EF Core provider with License Key
var rawConnectionString = builder.Configuration.GetConnectionString("PostgreSql")
    ?? throw new InvalidOperationException("Connection string 'PostgreSql' not found.");

var csBuilder = new Devart.Data.PostgreSql.PgSqlConnectionStringBuilder(rawConnectionString)
{
    LicenseKey = "Add LicenseKey"
};

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UsePostgreSql(csBuilder.ConnectionString);
});

// Register application services for Dependency Injection
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Devart MCP PostgreSQL Demo API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
