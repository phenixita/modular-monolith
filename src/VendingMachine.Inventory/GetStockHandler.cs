using MediatR;

namespace VendingMachine.Inventory;

internal sealed class GetStockHandler(IInventoryRepository repository)
    : IRequestHandler<GetStockQuery, int>
{
    public Task<int> Handle(GetStockQuery request, CancellationToken cancellationToken) =>
        repository.GetQuantityAsync(request.Code, cancellationToken);
}
