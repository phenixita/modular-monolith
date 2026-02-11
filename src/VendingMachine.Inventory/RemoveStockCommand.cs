using MediatR;

namespace VendingMachine.Inventory;

public sealed record RemoveStockCommand(string Code, int Quantity) : IRequest<Unit>;
