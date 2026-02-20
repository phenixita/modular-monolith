using MediatR;

namespace VendingMachine.Inventory.Stock.Set;

public sealed record SetStockCommand(string Code, int Quantity) : IRequest<Unit>;
