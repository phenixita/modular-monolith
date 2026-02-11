using MediatR;

namespace VendingMachine.Inventory;

public sealed record SetStockCommand(string Code, int Quantity) : IRequest<Unit>;
