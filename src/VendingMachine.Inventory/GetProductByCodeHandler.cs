using MediatR;

namespace VendingMachine.Inventory;

internal sealed class GetProductByCodeHandler(IInventoryRepository repository)
    : IRequestHandler<GetProductByCodeQuery, Product>
{
    public Task<Product> Handle(GetProductByCodeQuery request, CancellationToken cancellationToken) =>
        repository.GetByCodeAsync(request.Code, cancellationToken);
}
