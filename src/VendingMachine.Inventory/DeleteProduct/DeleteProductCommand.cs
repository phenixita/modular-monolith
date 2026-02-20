using MediatR;

namespace VendingMachine.Inventory.DeleteProduct;

public sealed record DeleteProductCommand(string Code) : IRequest<Unit>;
