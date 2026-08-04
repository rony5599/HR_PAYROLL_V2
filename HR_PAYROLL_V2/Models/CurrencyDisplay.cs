namespace HR_PAYROLL_V2.Models;

public static class CurrencyDisplay
{
    private static readonly Dictionary<string, string> Symbols = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = "$",
        ["BDT"] = "৳",
        ["EUR"] = "€",
        ["GBP"] = "£",
        ["INR"] = "₹",
    };

    public static string Format(decimal amount, string? currencyCode)
    {
        var symbol = currencyCode is not null && Symbols.TryGetValue(currencyCode, out var s) ? s : (currencyCode ?? "$") + " ";
        return $"{symbol}{amount:N2}";
    }
}
