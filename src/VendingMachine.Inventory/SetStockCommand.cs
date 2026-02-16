using MediatR;

namespace VendingMachine.Inventory;

internal sealed record SetStockCommand(string Code, int Quantity) : IRequest<Unit>;
