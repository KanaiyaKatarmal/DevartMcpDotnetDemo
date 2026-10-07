using DevartMcpDotnetDemo.DTOs;

namespace DevartMcpDotnetDemo.Services;

public interface ICustomerService
{
    Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(int count = 10, CancellationToken cancellationToken = default);
}
