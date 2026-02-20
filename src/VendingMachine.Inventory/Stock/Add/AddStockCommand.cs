using MediatR;

namespace VendingMachine.Inventory.Stock.Add;

public sealed record AddStockCommand(string Code, int Quantity) : IRequest<Unit>;
