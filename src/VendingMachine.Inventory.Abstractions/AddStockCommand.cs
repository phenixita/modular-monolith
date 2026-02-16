using MediatR;

namespace VendingMachine.Inventory;

public sealed record AddStockCommand(string Code, int Quantity) : IRequest<Unit>;
