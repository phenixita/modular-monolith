using MediatR;

namespace VendingMachine.Inventory.CreateProduct;

public sealed record CreateProductCommand(Product Product) : IRequest<Unit>;
