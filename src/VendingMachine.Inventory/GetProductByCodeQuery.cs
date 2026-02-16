using MediatR;

namespace VendingMachine.Inventory;

internal sealed record GetProductByCodeQuery(string Code) : IRequest<Product>;
