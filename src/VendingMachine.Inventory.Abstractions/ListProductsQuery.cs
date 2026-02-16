using MediatR;

namespace VendingMachine.Inventory;

public sealed record ListProductsQuery() : IRequest<IReadOnlyCollection<Product>>;
