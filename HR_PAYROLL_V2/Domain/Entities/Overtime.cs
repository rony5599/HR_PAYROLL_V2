using HR_PAYROLL_V2.Domain.Common;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Domain.Entities;

public class OvertimePolicy : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal MinOvertimeMinutes { get; set; } = 30;
    public decimal MaxDailyHours { get; set; } = 4;
    public decimal MaxMonthlyHours { get; set; } = 60;
    public decimal NormalMultiplier { get; set; } = 1.5m;
    public decimal WeekendMultiplier { get; set; } = 2.0m;
    public decimal HolidayMultiplier { get; set; } = 2.0m;
    public bool RequiresApproval { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class OvertimeRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateOnly OvertimeDate { get; set; }
    public decimal Hours { get; set; }
    public string Reason { get; set; } = string.Empty;

    public OvertimeRequestStatus Status { get; set; } = OvertimeRequestStatus.Pending;
    public Guid? ApproverId { get; set; }
    public Employee? Approver { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionComment { get; set; }
}
