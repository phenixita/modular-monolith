using MediatR;

namespace VendingMachine.Inventory.ListProducts;

public sealed record ListProductsQuery() : IRequest<IReadOnlyCollection<Product>>;
