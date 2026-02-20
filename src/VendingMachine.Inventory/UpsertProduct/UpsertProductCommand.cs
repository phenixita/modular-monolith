using MediatR;

namespace VendingMachine.Inventory.UpsertProduct;

public sealed record UpsertProductCommand(Product Product) : IRequest<Unit>;
