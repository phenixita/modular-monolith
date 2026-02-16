using MediatR;

namespace VendingMachine.Inventory;

internal sealed record AddStockCommand(string Code, int Quantity) : IRequest<Unit>;
