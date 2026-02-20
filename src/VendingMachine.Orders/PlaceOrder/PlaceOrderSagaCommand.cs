using MediatR;

namespace VendingMachine.Orders.PlaceOrder;

public sealed record PlaceOrderSagaCommand(string Code) : IRequest<OrderReceipt>;
