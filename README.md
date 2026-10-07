# Devart dotConnect PostgreSQL + EF Core + .NET 10 Web API Demo

A high-performance ASP.NET Core Web API proof of concept built with **.NET 10** and **Entity Framework Core**, utilizing **Devart dotConnect for PostgreSQL** (`Devart.Data.PostgreSql.EFCore`) as the database provider.

The database schema and relationship model were discovered and verified via the **Devart PostgreSQL MCP Server**.

---

## Features

- **Framework**: .NET 10 (`net10.0`) Web API.
- **ORM**: Entity Framework Core with pure LINQ database-side aggregation.
- **Provider**: Devart dotConnect for PostgreSQL Professional (`Devart.Data.PostgreSql.EFCore`).
- **Architecture**: Clean, production-oriented structure (Controllers, Services with DI, DbContext, Models, DTOs).
- **Performance**: `AsNoTracking()` read-only queries with PostgreSQL server-side aggregation (`SUM`, `COUNT`, `ORDER BY`, `LIMIT`).
- **Interactive Documentation**: OpenAPI & Swagger UI powered by `Swashbuckle.AspNetCore`.
- **Secure Configuration**: Externalized connection strings and runtime licensing via `PgSqlConnectionStringBuilder`.

---

## Project Structure

```text
DevartMcpDotnetDemo/
├── Controllers/
│   └── CustomersController.cs      # API Controller exposing GET /api/customers/top
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext with Fluent API configuration
├── DTOs/
│   └── TopCustomerDto.cs           # Data Transfer Object for top customer results
├── Models/
│   ├── Customer.cs                 # Entity model mapped to public.customers
│   └── Order.cs                    # Entity model mapped to public.orders
├── Services/
│   ├── ICustomerService.cs         # Service contract for customer business logic
│   └── CustomerService.cs          # Pure EF Core LINQ aggregation implementation
├── appsettings.json                # Application configuration
├── appsettings.Development.json    # Development logging configuration
├── Program.cs                      # Dependency Injection, License, Swagger & Pipeline setup
├── DevartMcpDotnetDemo.csproj      # .NET 10 project file & package references
└── README.md                       # Documentation
```

---

## Database Schema (`DevartMcpDemo`)

The project interacts with the `public` schema in the `DevartMcpDemo` PostgreSQL database:

### 1. `public.customers`
| Column Name | Data Type | Constraints |
| :--- | :--- | :--- |
| `id` | `integer` | Primary Key (`customers_pkey`) |
| `name` | `character varying(100)` | Not Null |
| `email` | `character varying(150)` | Not Null |
| `created_at` | `timestamp` | Not Null, Default `now()` |

### 2. `public.orders`
| Column Name | Data Type | Constraints |
| :--- | :--- | :--- |
| `id` | `integer` | Primary Key (`orders_pkey`) |
| `customer_id` | `integer` | Foreign Key &rarr; `public.customers(id)` |
| `order_date` | `date` | Not Null (Mapped to C# `DateTime`) |
| `total_amount` | `decimal(12,2)` | Not Null |
| `status` | `character varying(20)` | Not Null (e.g. `'Completed'`) |

---

## Pure EF Core LINQ Query Execution

The endpoint aggregates completed orders on the PostgreSQL server without loading full tables into memory:

```csharp
public async Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(int count = 10, CancellationToken cancellationToken = default)
{
    return await _context.Customers
        .AsNoTracking()
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Email,
            TotalOrders = c.Orders.Count(o => o.Status == "Completed"),
            TotalOrderValue = c.Orders.Where(o => o.Status == "Completed").Sum(o => (decimal?)o.TotalAmount) ?? 0m
        })
        .Where(x => x.TotalOrders > 0)
        .OrderByDescending(x => x.TotalOrderValue)
        .Take(count)
        .Select(x => new TopCustomerDto(
            x.Id,
            x.Name,
            x.Email,
            x.TotalOrders,
            x.TotalOrderValue
        ))
        .ToListAsync(cancellationToken);
}
```

### Generated PostgreSQL SQL:
```sql
SELECT 
    c.id, 
    c."name", 
    c.email, 
    (SELECT COUNT(*) FROM public.orders AS o1 WHERE c.id = o1.customer_id AND o1.status = 'Completed'), 
    COALESCE((SELECT COALESCE(SUM(o2.total_amount), 0) FROM public.orders AS o2 WHERE c.id = o2.customer_id AND o2.status = 'Completed'), 0)
FROM public.customers AS c
WHERE (SELECT COUNT(*) FROM public.orders AS o WHERE c.id = o.customer_id AND o.status = 'Completed') > 0
ORDER BY COALESCE((SELECT COALESCE(SUM(o0.total_amount), 0) FROM public.orders AS o0 WHERE c.id = o0.customer_id AND o0.status = 'Completed'), 0) DESC
LIMIT :p;
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- PostgreSQL Server 14+ with `DevartMcpDemo` database
- Devart dotConnect for PostgreSQL License Key

---

## Configuration

### 1. Connection String (`appsettings.json`)
Configure your PostgreSQL connection parameters:

```json
{
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Port=5432;Database=DevartMcpDemo;User Id=postgres;Password=YOUR_PASSWORD;"
  }
}
```

Or configure via .NET user-secrets:
```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=DevartMcpDemo;User Id=postgres;Password=YOUR_PASSWORD;"
```

### 2. Devart License Key
In `Program.cs`, the runtime license key is assigned via `PgSqlConnectionStringBuilder`:

```csharp
var csBuilder = new Devart.Data.PostgreSql.PgSqlConnectionStringBuilder(rawConnectionString)
{
    LicenseKey = "<YOUR_DOTCONNECT_POSTGRESQL_KEY>"
};

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UsePostgreSql(csBuilder.ConnectionString);
});
```

---

## Getting Started

### 1. Restore Dependencies
```powershell
dotnet restore
```

### 2. Build Solution
```powershell
dotnet build
```

### 3. Run Web API
```powershell
dotnet run --urls "http://localhost:5000"
```

---

## API Testing & Swagger

### 1. Swagger UI
Navigate to either:
- **Swagger Documentation:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Root URL (redirects to Swagger):** [http://localhost:5000/](http://localhost:5000/)

### 2. Endpoint: Top 10 Customers
- **Method:** `GET`
- **Route:** `/api/customers/top`
- **cURL Example:**
  ```powershell
  curl http://localhost:5000/api/customers/top
  ```

### 3. Sample JSON Response
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
  },
  {
    "customerId": 3,
    "name": "Rahul Mehta",
    "email": "rahul@example.com",
    "totalOrders": 3,
    "totalOrderValue": 28000.00
  },
  {
    "customerId": 13,
    "name": "Aditya Gupta",
    "email": "aditya@example.com",
    "totalOrders": 2,
    "totalOrderValue": 28000.00
  },
  {
    "customerId": 1,
    "name": "Aarav Sharma",
    "email": "aarav@example.com",
    "totalOrders": 3,
    "totalOrderValue": 27700.00
  },
  {
    "customerId": 11,
    "name": "Karan Singh",
    "email": "karan@example.com",
    "totalOrders": 2,
    "totalOrderValue": 27500.00
  },
  {
    "customerId": 2,
    "name": "Priya Patel",
    "email": "priya@example.com",
    "totalOrders": 2,
    "totalOrderValue": 26000.00
  },
  {
    "customerId": 7,
    "name": "Rohan Kulkarni",
    "email": "rohan@example.com",
    "totalOrders": 2,
    "totalOrderValue": 22500.00
  },
  {
    "customerId": 8,
    "name": "Neha Iyer",
    "email": "neha@example.com",
    "totalOrders": 2,
    "totalOrderValue": 14500.00
  },
  {
    "customerId": 12,
    "name": "Meera Rao",
    "email": "meera@example.com",
    "totalOrders": 2,
    "totalOrderValue": 14000.00
  }
]
```

---

## License

This proof of concept uses Devart dotConnect for PostgreSQL. Ensure you possess an active license or trial key when deploying or running in production environments.
