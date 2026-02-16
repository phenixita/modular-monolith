using MediatR;

namespace VendingMachine.Inventory;

internal sealed record ListProductsQuery() : IRequest<IReadOnlyCollection<Product>>;
