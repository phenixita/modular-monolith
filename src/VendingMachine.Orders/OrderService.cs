using MediatR;

namespace VendingMachine.Orders;

public sealed class OrderService(IMediator mediator) : IOrderService
{
    public Task<OrderReceipt> PlaceOrder(string code, CancellationToken cancellationToken = default) =>
        mediator.Send(new PlaceOrderCommand(code), cancellationToken);
}
