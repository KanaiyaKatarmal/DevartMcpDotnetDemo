using DevartMcpDotnetDemo.DTOs;
using DevartMcpDotnetDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevartMcpDotnetDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Returns the top 10 customers by total completed order value.
    /// </summary>
    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyList<TopCustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TopCustomerDto>>> GetTopCustomers(CancellationToken cancellationToken)
    {
        var topCustomers = await _customerService.GetTopCustomersAsync(10, cancellationToken);
        return Ok(topCustomers);
    }
}
