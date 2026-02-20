using MediatR;

namespace VendingMachine.Inventory.RemoveStock;

public sealed record RemoveStockCommand(string Code, int Quantity) : IRequest<Unit>;
