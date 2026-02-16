using MediatR;

namespace VendingMachine.Inventory;

internal sealed record RemoveStockCommand(string Code, int Quantity) : IRequest<Unit>;
