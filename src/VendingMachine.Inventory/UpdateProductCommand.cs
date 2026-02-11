using MediatR;

namespace VendingMachine.Inventory;

public sealed record UpdateProductCommand(Product Product) : IRequest<Unit>;
