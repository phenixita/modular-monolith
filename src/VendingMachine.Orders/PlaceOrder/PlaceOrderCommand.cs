using MediatR;

namespace VendingMachine.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(string Code) : IRequest<OrderReceipt>;
