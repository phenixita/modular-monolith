using MediatR;

namespace VendingMachine.Inventory.ListProducts;

internal sealed class ListProductsHandler(IInventoryRepository repository)
    : IRequestHandler<ListProductsQuery, IReadOnlyCollection<Product>>
{
    public Task<IReadOnlyCollection<Product>> Handle(ListProductsQuery request, CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}
