using MediatR;

namespace VendingMachine.Orders;

public sealed record PlaceOrderSagaCommand(string Code) : IRequest<OrderReceipt>;
