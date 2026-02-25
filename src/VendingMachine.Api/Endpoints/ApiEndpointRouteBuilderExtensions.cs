namespace VendingMachine.Api.Endpoints;

internal static class ApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var api = endpointRouteBuilder.MapGroup("/api");
        api.MapInventoryEndpoints();
        api.MapCashEndpoints();
        api.MapOrderEndpoints();
        api.MapReportingEndpoints();

        return endpointRouteBuilder;
    }
}
