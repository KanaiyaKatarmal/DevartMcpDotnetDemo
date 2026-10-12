# Devart dotConnect for PostgreSQL + EF Core + .NET 10 Web API Demo

A high-performance ASP.NET Core Web API built with **.NET 10** and **Entity Framework Core**, using **[Devart dotConnect for PostgreSQL](https://www.devart.com/dotconnect/postgresql/?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal)** (`Devart.Data.PostgreSql.EFCore`) as the database provider.

The PostgreSQL schema and relationships were discovered and verified with the **ADO.NET-based Devart PostgreSQL MCP Server**. This keeps the .NET workflow on the ADO.NET + dotConnect stack without requiring an ODBC driver or DSN.

---

## Features

- **Framework:** .NET 10 (`net10.0`) Web API.
- **ORM:** Entity Framework Core with LINQ-based, database-side aggregation.
- **Provider:** [Devart dotConnect for PostgreSQL](https://www.devart.com/dotconnect/postgresql/?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal) (`Devart.Data.PostgreSql.EFCore`).
- **MCP server:** [Devart.Mcp.PostgreSql on NuGet](https://www.nuget.org/packages/Devart.Mcp.PostgreSql), the ADO.NET-based PostgreSQL MCP server.
- **Architecture:** Controllers, services with dependency injection, DbContext, models, and DTOs.
- **Performance:** `AsNoTracking()` read-only queries and PostgreSQL-side `SUM`, `COUNT`, ordering, and limiting.
- **Interactive documentation:** OpenAPI and Swagger UI powered by `Swashbuckle.AspNetCore`.
- **Configuration:** Connection strings and the dotConnect license key are configured outside application source code using `PgSqlConnectionStringBuilder`.

---

## Project Structure

```text
DevartMcpDotnetDemo/
├── Controllers/
│   └── CustomersController.cs      # GET /api/customers/top
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext and Fluent API mapping
├── DTOs/
│   └── TopCustomerDto.cs           # API response DTO
├── Models/
│   ├── Customer.cs                 # Maps to public.customers
│   └── Order.cs                    # Maps to public.orders
├── Services/
│   ├── ICustomerService.cs         # Service contract
│   └── CustomerService.cs          # EF Core LINQ aggregation
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── DevartMcpDotnetDemo.csproj
└── README.md
```

---

## Database Schema (`DevartMcpDemo`)

The application reads from the `public` schema in the `DevartMcpDemo` PostgreSQL database.

### `public.customers`

| Column | Data type | Constraints |
|---|---|---|
| `id` | `integer` | Primary key (`customers_pkey`) |
| `name` | `character varying(100)` | Not null |
| `email` | `character varying(150)` | Not null |
| `created_at` | `timestamp with time zone` | Not null; default `now()` |

### `public.orders`

| Column | Data type | Constraints |
|---|---|---|
| `id` | `integer` | Primary key (`orders_pkey`) |
| `customer_id` | `integer` | Foreign key to `public.customers(id)` |
| `order_date` | `date` | Not null; mapped to C# `DateTime` |
| `total_amount` | `numeric(12,2)` | Not null |
| `status` | `character varying(20)` | Not null; for example, `Completed` |

---

## How the Query Works

The endpoint returns the top 10 customers by the total value of their **completed** orders. Counting and summing are performed by PostgreSQL through the EF Core query; the application does not load every order into memory to calculate the totals.

```csharp
public async Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(
    int count = 10,
    CancellationToken cancellationToken = default)
{
    return await _context.Customers
        .AsNoTracking()
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Email,
            TotalOrders = c.Orders.Count(o => o.Status == "Completed"),
            TotalOrderValue = c.Orders
                .Where(o => o.Status == "Completed")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m
        })
        .Where(x => x.TotalOrders > 0)
        .OrderByDescending(x => x.TotalOrderValue)
        .Take(count)
        .Select(x => new TopCustomerDto(
            x.Id,
            x.Name,
            x.Email,
            x.TotalOrders,
            x.TotalOrderValue))
        .ToListAsync(cancellationToken);
}
```

The generated SQL is provider-dependent. To inspect the exact SQL produced by your installed provider version, enable EF Core SQL logging or inspect the query with `ToQueryString()` during development.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- PostgreSQL 14 or later, with the `DevartMcpDemo` database and the tables above
- [Devart dotConnect for PostgreSQL](https://www.devart.com/dotconnect/postgresql/?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal) and a valid license/trial key
- For schema discovery with MCP: [.NET 8.0 runtime or later](https://dotnet.microsoft.com/) and [Devart.Mcp.PostgreSql on NuGet](https://www.nuget.org/packages/Devart.Mcp.PostgreSql)

### Install the ADO.NET-based Devart PostgreSQL MCP Server

```powershell
dotnet tool install --global Devart.Mcp.PostgreSql
```

The package includes dotConnect for PostgreSQL; no separate ODBC driver or DSN is required for this ADO.NET-based MCP server.

The default connection-profile configuration file is:

```text
%ProgramData%\Devart\AdoNet MCP Server for PostgreSQL\mcpserver.json
```

Configure a profile for your PostgreSQL database, then select that profile in your MCP client. Keep database passwords and license keys out of source control.

For connection-string options, see the [dotConnect for PostgreSQL connection string reference](https://www.devart.com/dotconnect/postgresql/connection-strings.html?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal).

For source code and the ADO.NET implementation, see the [Devart PostgreSQL MCP Server repository](https://github.com/devart-ai-connectivity/devart-mcp-server-postgresql).

If you work with other data sources or already have ODBC drivers installed, see [Devart MCP Server Universal](https://www.devart.com/mcp/universal/?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal).

---

## Configuration

### 1. PostgreSQL connection string

Set the connection string in User Secrets for local development rather than committing a password to `appsettings.json`.

From the project directory:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Server=localhost;Port=5432;Database=DevartMcpDemo;User Id=postgres;Password=YOUR_PASSWORD;"
```

Use your own server, database, and user details. The connection-string format and available options are documented in the [dotConnect for PostgreSQL connection string reference](https://www.devart.com/dotconnect/postgresql/connection-strings.html?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal).

### 2. Devart license key

The application configures the runtime license key through `PgSqlConnectionStringBuilder`. Store the key in User Secrets or an environment variable; do not hard-code a real key or commit it to source control.

Illustrative `Program.cs` configuration:

```csharp
var rawConnectionString =
    builder.Configuration.GetConnectionString("PostgreSql")
    ?? throw new InvalidOperationException(
        "Connection string 'PostgreSql' is not configured.");

var licenseKey =
    builder.Configuration["Devart:LicenseKey"]
    ?? throw new InvalidOperationException(
        "Devart license key is not configured.");

var csBuilder =
    new Devart.Data.PostgreSql.PgSqlConnectionStringBuilder(rawConnectionString)
    {
        LicenseKey = licenseKey
    };

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UsePostgreSql(csBuilder.ConnectionString);
});
```

Set the key locally with User Secrets:

```powershell
dotnet user-secrets set "Devart:LicenseKey" "YOUR_DOTCONNECT_POSTGRESQL_KEY"
```

Adapt this snippet to the existing `Program.cs` if the project already configures the provider or DbContext.

---

## Getting Started

### 1. Restore dependencies

```powershell
dotnet restore
```

### 2. Build

```powershell
dotnet build
```

### 3. Run the API

```powershell
dotnet run --urls "http://localhost:5000"
```

---

## API Testing and Swagger

### Swagger UI

- **Swagger UI:** <http://localhost:5000/swagger>
- **Root URL:** <http://localhost:5000/> (if root redirect is configured)

### Endpoint: Top 10 customers

- **Method:** `GET`
- **Route:** `/api/customers/top`
- **Local URL:** <http://localhost:5000/api/customers/top>

PowerShell/cURL example:

```powershell
curl http://localhost:5000/api/customers/top
```

### Sample response

The following illustrates the response shape and example values. The actual response depends on the contents of your database.

```json
[
  {
    "customerId": 5,
    "name": "Vikram Shah",
    "email": "vikram@example.com",
    "totalOrders": 2,
    "totalOrderValue": 40000.00
  },
  {
    "customerId": 9,
    "name": "Arjun Nair",
    "email": "arjun@example.com",
    "totalOrders": 2,
    "totalOrderValue": 31000.00
  }
]
```

---

## Related Links

- [dotConnect for PostgreSQL](https://www.devart.com/dotconnect/postgresql/?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal)
- [Devart.Mcp.PostgreSql on NuGet](https://www.nuget.org/packages/Devart.Mcp.PostgreSql)
- [dotConnect for PostgreSQL connection string reference](https://www.devart.com/dotconnect/postgresql/connection-strings.html?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal)
- [Devart MCP Server Universal](https://www.devart.com/mcp/universal/?utm_source=kanaiyakatarmal.com&utm_medium=referral&utm_campaign=cc_pr_kanaiyakatarmal)
- [Devart PostgreSQL MCP Server source repository](https://github.com/devart-ai-connectivity/devart-mcp-server-postgresql)

---

## Suggested GitHub Topics

```text
mcp
mcp-server
postgresql
dotnet
ado-net
```

---

## License

This proof of concept uses Devart dotConnect for PostgreSQL. Ensure you have a valid license or trial key for your intended use.
