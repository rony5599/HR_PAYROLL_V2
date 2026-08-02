using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class Holiday : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public HolidayType Type { get; set; } = HolidayType.PaidPublic;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
