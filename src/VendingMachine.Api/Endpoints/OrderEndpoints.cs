using VendingMachine.Api.Contracts;
using VendingMachine.Api.Shared;
using VendingMachine.Orders;

namespace VendingMachine.Api.Endpoints;

internal static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost("/orders", PlaceOrderAsync);

        return api;
    }

    private static async Task<IResult> PlaceOrderAsync(
        PlaceOrderRequest request,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        var code = ProductCodeNormalizer.Normalize(request.Code);
        var receipt = await orderService.PlaceOrder(code, cancellationToken);

        var data = new PlaceOrderResponse(
            receipt.ProductCode,
            receipt.Price,
            receipt.Balance,
            receipt.Stock);

        return Results.Ok(ApiEnvelope.Success(data));
    }
}
