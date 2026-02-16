using MediatR;

namespace VendingMachine.Inventory;

internal sealed record UpsertProductCommand(Product Product) : IRequest<Unit>;
