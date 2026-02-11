using MediatR;

namespace VendingMachine.Inventory;

public sealed record GetStockQuery(string Code) : IRequest<int>;
