using MediatR;

namespace VendingMachine.Orders;

public sealed record PlaceOrderCommand(string Code) : IRequest<OrderReceipt>;
