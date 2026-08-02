using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class Shift : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ShiftType Type { get; set; } = ShiftType.Fixed;

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int BreakMinutes { get; set; }
    public decimal RequiredHours { get; set; }
    public decimal HalfDayHours { get; set; }
    public decimal OvertimeAfterHours { get; set; }

    public int LateGraceMinutes { get; set; }
    public int EarlyExitGraceMinutes { get; set; }
    public int CheckInWindowMinutes { get; set; }
    public int CheckOutWindowMinutes { get; set; }

    public WorkingDays WorkingDays { get; set; } =
        WorkingDays.Sunday | WorkingDays.Monday | WorkingDays.Tuesday | WorkingDays.Wednesday | WorkingDays.Thursday;

    public bool CrossesMidnight { get; set; }
    public bool CalculateOvertime { get; set; } = true;
    public bool HonorHolidayProtection { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class ShiftAssignment : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public DateOnly EffectiveStartDate { get; set; }
    public DateOnly? EffectiveEndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
