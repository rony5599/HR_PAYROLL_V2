using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Payroll;

public class OvertimePolicyViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 240)]
    public decimal MinOvertimeMinutes { get; set; } = 30;

    [Range(0, 24)]
    public decimal MaxDailyHours { get; set; } = 4;

    [Range(0, 300)]
    public decimal MaxMonthlyHours { get; set; } = 60;

    [Range(1, 5)]
    public decimal NormalMultiplier { get; set; } = 1.5m;

    [Range(1, 5)]
    public decimal WeekendMultiplier { get; set; } = 2.0m;

    [Range(1, 5)]
    public decimal HolidayMultiplier { get; set; } = 2.0m;

    public bool RequiresApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
