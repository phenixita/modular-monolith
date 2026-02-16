using MediatR;

namespace VendingMachine.Inventory;

internal sealed record CreateProductCommand(Product Product) : IRequest<Unit>;
