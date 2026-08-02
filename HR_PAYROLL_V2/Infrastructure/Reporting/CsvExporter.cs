using System.Text;

namespace HR_PAYROLL_V2.Infrastructure.Reporting;

public static class CsvExporter
{
    public static byte[] Build(IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(Escape)));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(v => Escape(v?.ToString() ?? string.Empty))));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
