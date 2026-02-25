using VendingMachine.Api.Contracts;
using VendingMachine.Reporting.Abstractions;

namespace VendingMachine.Api.Endpoints;

internal static class ReportingEndpoints
{
    public static RouteGroupBuilder MapReportingEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/reporting/dashboard", GetDashboardAsync);

        return api;
    }

    private static async Task<IResult> GetDashboardAsync(
        IReportingService reportingService,
        CancellationToken cancellationToken)
    {
        var stats = await reportingService.GetDashboardStatsAsync(cancellationToken);
        var data = new DashboardStatsResponse(
            stats.TotaleOrdiniEuro,
            stats.TotaleNumeroOrdini,
            stats.MediaEuroOrdini);

        return Results.Ok(ApiEnvelope.Success(data));
    }
}
