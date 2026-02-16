namespace VendingMachine.Orders;

public interface IOrderService
{
    Task<OrderReceipt> PlaceOrder(string code, CancellationToken cancellationToken = default);
}
