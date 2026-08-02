using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Leave;

public class LeaveTypeViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Range(0, 365)]
    public decimal AnnualEntitlementDays { get; set; } = 10;

    [Range(1, 365)]
    public int MaxConsecutiveDays { get; set; } = 10;

    [Range(0, 90)]
    public int AdvanceNoticeDays { get; set; } = 2;

    public bool RequiresAttachment { get; set; }
    public bool ExcludeWeekends { get; set; } = true;
    public bool ExcludeHolidays { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
