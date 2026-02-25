namespace VendingMachine.Reporting.Abstractions;

public sealed record DashboardStats(
    decimal TotaleOrdiniEuro,
    int TotaleNumeroOrdini,
    decimal MediaEuroOrdini);
