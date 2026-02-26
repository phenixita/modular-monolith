namespace VendingMachine.Reporting.Abstractions;

public sealed record DashboardStats(
    decimal TotalRevenue,
    int OrderCount,
    decimal AverageOrderValue);
