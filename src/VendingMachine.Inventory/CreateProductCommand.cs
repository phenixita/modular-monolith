using MediatR;

namespace VendingMachine.Inventory;

public sealed record CreateProductCommand(Product Product) : IRequest<Unit>;
