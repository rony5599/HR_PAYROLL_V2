using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class Attendance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid? ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public DateOnly AttendanceDate { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }

    public decimal WorkedHours { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyExitMinutes { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public AttendanceSource Source { get; set; } = AttendanceSource.Manual;
    public string? Remarks { get; set; }
}

public class AttendanceRegularization : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateOnly AttendanceDate { get; set; }
    public TimeOnly? RequestedCheckIn { get; set; }
    public TimeOnly? RequestedCheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;

    public RegularizationStatus Status { get; set; } = RegularizationStatus.Pending;
    public Guid? ApproverId { get; set; }
    public Employee? Approver { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionComment { get; set; }
}
