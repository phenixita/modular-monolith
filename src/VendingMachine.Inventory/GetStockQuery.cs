using MediatR;

namespace VendingMachine.Inventory;

internal sealed record GetStockQuery(string Code) : IRequest<int>;
