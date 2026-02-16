using MediatR;

namespace VendingMachine.Inventory;

internal sealed record UpdateProductCommand(Product Product) : IRequest<Unit>;
