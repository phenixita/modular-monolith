using MediatR;

namespace VendingMachine.Inventory;

public sealed record DeleteProductCommand(string Code) : IRequest<Unit>;
