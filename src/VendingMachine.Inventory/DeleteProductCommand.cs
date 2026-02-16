using MediatR;

namespace VendingMachine.Inventory;

internal sealed record DeleteProductCommand(string Code) : IRequest<Unit>;
