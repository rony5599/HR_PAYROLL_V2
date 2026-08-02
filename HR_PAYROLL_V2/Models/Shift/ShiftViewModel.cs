using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Shift;

public class ShiftViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    public ShiftType Type { get; set; } = ShiftType.Fixed;

    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; } = new(9, 0);

    [DataType(DataType.Time)]
    public TimeOnly EndTime { get; set; } = new(17, 30);

    [Range(0, 480)]
    public int BreakMinutes { get; set; } = 30;

    [Range(0, 24)]
    public decimal RequiredHours { get; set; } = 8;

    [Range(0, 24)]
    public decimal HalfDayHours { get; set; } = 4;

    [Range(0, 24)]
    public decimal OvertimeAfterHours { get; set; } = 8.5m;

    [Range(0, 240)]
    public int LateGraceMinutes { get; set; } = 15;

    [Range(0, 240)]
    public int EarlyExitGraceMinutes { get; set; } = 10;

    [Range(0, 720)]
    public int CheckInWindowMinutes { get; set; } = 120;

    [Range(0, 720)]
    public int CheckOutWindowMinutes { get; set; } = 240;

    public bool Sunday { get; set; } = true;
    public bool Monday { get; set; } = true;
    public bool Tuesday { get; set; } = true;
    public bool Wednesday { get; set; } = true;
    public bool Thursday { get; set; } = true;
    public bool Friday { get; set; }
    public bool Saturday { get; set; }

    public bool CrossesMidnight { get; set; }
    public bool CalculateOvertime { get; set; } = true;
    public bool HonorHolidayProtection { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public WorkingDays ToWorkingDays()
    {
        var days = WorkingDays.None;
        if (Sunday) days |= WorkingDays.Sunday;
        if (Monday) days |= WorkingDays.Monday;
        if (Tuesday) days |= WorkingDays.Tuesday;
        if (Wednesday) days |= WorkingDays.Wednesday;
        if (Thursday) days |= WorkingDays.Thursday;
        if (Friday) days |= WorkingDays.Friday;
        if (Saturday) days |= WorkingDays.Saturday;
        return days;
    }

    public void FromWorkingDays(WorkingDays days)
    {
        Sunday = days.HasFlag(WorkingDays.Sunday);
        Monday = days.HasFlag(WorkingDays.Monday);
        Tuesday = days.HasFlag(WorkingDays.Tuesday);
        Wednesday = days.HasFlag(WorkingDays.Wednesday);
        Thursday = days.HasFlag(WorkingDays.Thursday);
        Friday = days.HasFlag(WorkingDays.Friday);
        Saturday = days.HasFlag(WorkingDays.Saturday);
    }
}
