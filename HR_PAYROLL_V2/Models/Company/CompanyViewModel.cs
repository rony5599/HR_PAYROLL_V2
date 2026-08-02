using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Company;

public class CompanyViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    public string? RegistrationNumber { get; set; }
    public string? Address { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    [Required]
    public string PayrollCurrency { get; set; } = "USD";

    [Required]
    public string PayrollFrequency { get; set; } = "Monthly";

    public bool IsActive { get; set; } = true;

    public bool WeekendSunday { get; set; }
    public bool WeekendMonday { get; set; }
    public bool WeekendTuesday { get; set; }
    public bool WeekendWednesday { get; set; }
    public bool WeekendThursday { get; set; }
    public bool WeekendFriday { get; set; } = true;
    public bool WeekendSaturday { get; set; } = true;

    public WorkingDays ToWeekendDays()
    {
        var days = WorkingDays.None;
        if (WeekendSunday) days |= WorkingDays.Sunday;
        if (WeekendMonday) days |= WorkingDays.Monday;
        if (WeekendTuesday) days |= WorkingDays.Tuesday;
        if (WeekendWednesday) days |= WorkingDays.Wednesday;
        if (WeekendThursday) days |= WorkingDays.Thursday;
        if (WeekendFriday) days |= WorkingDays.Friday;
        if (WeekendSaturday) days |= WorkingDays.Saturday;
        return days;
    }

    public void FromWeekendDays(WorkingDays days)
    {
        WeekendSunday = days.HasFlag(WorkingDays.Sunday);
        WeekendMonday = days.HasFlag(WorkingDays.Monday);
        WeekendTuesday = days.HasFlag(WorkingDays.Tuesday);
        WeekendWednesday = days.HasFlag(WorkingDays.Wednesday);
        WeekendThursday = days.HasFlag(WorkingDays.Thursday);
        WeekendFriday = days.HasFlag(WorkingDays.Friday);
        WeekendSaturday = days.HasFlag(WorkingDays.Saturday);
    }
}
