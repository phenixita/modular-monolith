using System.Globalization;

internal static class CliParsing
{
    public static string EnsureCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }

    public static decimal ParseMoney(string input, string fieldName)
    {
        if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"{fieldName} must be a decimal number using '.' as decimal separator.");
        }

        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(fieldName, "Value must be zero or positive.");
        }

        return value;
    }

    public static int ParseQuantity(string input, string fieldName)
    {
        if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"{fieldName} must be an integer number.");
        }

        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(fieldName, "Value must be positive.");
        }

        return value;
    }

    public static string FormatMoney(decimal amount) =>
        amount.ToString("0.00", CultureInfo.InvariantCulture);
}
