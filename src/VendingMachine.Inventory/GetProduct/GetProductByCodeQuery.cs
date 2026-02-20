using MediatR;

namespace VendingMachine.Inventory.GetProduct;

public sealed record GetProductByCodeQuery(string Code) : IRequest<Product>;
