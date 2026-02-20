using MediatR;

namespace VendingMachine.Inventory.Stock.Get;

public sealed record GetStockQuery(string Code) : IRequest<int>;
