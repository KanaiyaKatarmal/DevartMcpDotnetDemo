using DevartMcpDotnetDemo.Data;
using DevartMcpDotnetDemo.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DevartMcpDotnetDemo.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

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
}
