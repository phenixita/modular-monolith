using VendingMachine.Cash;
using VendingMachine.Inventory;

namespace VendingMachine.Orders;

public sealed class OrderService
{
    private readonly CashRegister _cashRegister;
    private readonly InventoryCatalog _catalog;
    private readonly StockRoom _stockRoom;

    public OrderService(CashRegister cashRegister, InventoryCatalog catalog, StockRoom stockRoom)
    {
        _cashRegister = cashRegister ?? throw new ArgumentNullException(nameof(cashRegister));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _stockRoom = stockRoom ?? throw new ArgumentNullException(nameof(stockRoom));
    }

    public OrderResult Purchase(string productCode)
    {
        var product = _catalog.GetByCode(productCode);

        if (_stockRoom.GetQuantity(product.Code) <= 0)
        {
            return OrderResult.OutOfStock(product);
        }

        try
        {
            _cashRegister.Charge(product.Price);
        }
        catch (InvalidOperationException)
        {
            return OrderResult.InsufficientFunds(product, _cashRegister.Balance);
        }

        _stockRoom.RemoveStock(product.Code, 1);
        return OrderResult.Success(product);
    }
}
