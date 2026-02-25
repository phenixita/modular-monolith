using MediatR;

namespace VendingMachine.Orders;

public sealed record OrderConfirmed(string ProductCode, decimal Price, DateTimeOffset OrderedAt) : INotification;
