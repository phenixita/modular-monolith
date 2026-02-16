using MediatR;

namespace VendingMachine.Inventory;

public sealed record GetProductByCodeQuery(string Code) : IRequest<Product>;
