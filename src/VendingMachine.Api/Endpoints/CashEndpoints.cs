using VendingMachine.Api.Contracts;
using VendingMachine.Cash;

namespace VendingMachine.Api.Endpoints;

internal static class CashEndpoints
{
    public static RouteGroupBuilder MapCashEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost("/cash/insert", InsertCashAsync);
        api.MapGet("/cash/balance", GetBalanceAsync);
        api.MapPost("/cash/refund", RefundAsync);

        return api;
    }

    private static async Task<IResult> InsertCashAsync(
        InsertCashRequest request,
        ICashRegisterService cashService,
        CancellationToken cancellationToken)
    {
        await cashService.Insert(request.Amount, cancellationToken);
        var balance = await cashService.GetBalance(cancellationToken);

        var data = new BalanceResponse(balance);
        return Results.Ok(ApiEnvelope.Success(data));
    }

    private static async Task<IResult> GetBalanceAsync(
        ICashRegisterService cashService,
        CancellationToken cancellationToken)
    {
        var balance = await cashService.GetBalance(cancellationToken);
        var data = new BalanceResponse(balance);

        return Results.Ok(ApiEnvelope.Success(data));
    }

    private static async Task<IResult> RefundAsync(
        ICashRegisterService cashService,
        CancellationToken cancellationToken)
    {
        var refunded = await cashService.RefundAll(cancellationToken);
        var balance = await cashService.GetBalance(cancellationToken);

        var data = new RefundResponse(refunded, balance);
        return Results.Ok(ApiEnvelope.Success(data));
    }
}
