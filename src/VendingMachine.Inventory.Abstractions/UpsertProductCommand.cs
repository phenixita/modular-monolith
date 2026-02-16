using MediatR;

namespace VendingMachine.Inventory;

public sealed record UpsertProductCommand(Product Product) : IRequest<Unit>;
