namespace VendingMachine.Api.Shared;

internal static class ProductCodeNormalizer
{
    public static string Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }
}
