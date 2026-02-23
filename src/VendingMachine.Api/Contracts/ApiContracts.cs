namespace VendingMachine.Api.Contracts;

internal sealed record ApiEnvelope(object? Data, ApiError? Error)
{
    public static ApiEnvelope Success(object data) => new(data, null);

    public static ApiEnvelope Failure(string code, string message) =>
        new(null, new ApiError(code, message));
}

internal sealed record ApiError(string Code, string Message);

internal sealed record ProductCreatedResponse(string ProductCode, decimal Price);

internal sealed record StockResponse(string ProductCode, int Stock);

internal sealed record BalanceResponse(decimal Balance);

internal sealed record RefundResponse(decimal RefundedAmount, decimal Balance);

internal sealed record PlaceOrderResponse(string ProductCode, decimal Price, decimal Balance, int Stock);

internal sealed record CreateProductRequest(string? Code, decimal Price);

internal sealed record LoadStockRequest(int Quantity);

internal sealed record InsertCashRequest(decimal Amount);

internal sealed record PlaceOrderRequest(string? Code);
