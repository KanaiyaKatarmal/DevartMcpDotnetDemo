namespace DevartMcpDotnetDemo.DTOs;

public sealed record TopCustomerDto(
    int CustomerId,
    string Name,
    string Email,
    int TotalOrders,
    decimal TotalOrderValue
);
