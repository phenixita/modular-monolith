using MediatR;

namespace VendingMachine.Inventory.UpdateProduct;

public sealed record UpdateProductCommand(Product Product) : IRequest<Unit>;
