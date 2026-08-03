using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.AuditLog;

public class AuditLogListViewModel
{
    public IReadOnlyList<HR_PAYROLL_V2.Domain.Entities.AuditLog> Items { get; set; } = Array.Empty<HR_PAYROLL_V2.Domain.Entities.AuditLog>();
    public IReadOnlyList<string> EntityNames { get; set; } = Array.Empty<string>();

    public string? EntityName { get; set; }
    public AuditAction? Action { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
}
