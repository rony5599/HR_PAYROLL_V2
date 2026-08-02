namespace HR_PAYROLL_V2.Domain.Enums;

public enum EmployeeStatus
{
    Active,
    Inactive,
    OnProbation,
    Confirmed,
    Suspended,
    OnNotice,
    Resigned,
    Terminated,
    Retired,
    ContractExpired,
    Separated
}

public enum OrganizationalUnitLevel
{
    Division,
    BusinessUnit,
    Department,
    Section,
    Team,
    Branch,
    Location,
    CostCenter
}

public enum ReportingRelationshipType
{
    Primary,
    DottedLine,
    Acting,
    Delegated
}

public enum ShiftType
{
    Fixed,
    Flexible,
    Rotating,
    Overnight,
    Split,
    NoFixedShift
}

[Flags]
public enum WorkingDays
{
    None = 0,
    Sunday = 1,
    Monday = 2,
    Tuesday = 4,
    Wednesday = 8,
    Thursday = 16,
    Friday = 32,
    Saturday = 64
}

public enum OvertimeRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public enum SalaryComponentType
{
    Earning,
    Deduction
}

public enum SalaryCalculationMethod
{
    FixedAmount,
    PercentageOfBasic
}

public enum ProrationMethod
{
    WorkingDays,
    CalendarDays,
    Fixed30Days
}

public enum PayrollPeriodStatus
{
    Draft,
    Finalized,
    Locked
}

public enum HolidayType
{
    PaidPublic,
    Company,
    Religious,
    Regional,
    Optional,
    HalfDay
}

public enum AttendanceStatus
{
    Present,
    Late,
    EarlyDeparture,
    HalfDay,
    Incomplete,
    Absent,
    OnLeave,
    Holiday,
    WorkFromHome,
    OfficialDuty
}

public enum AttendanceSource
{
    Biometric,
    Web,
    Mobile,
    Manual,
    Import,
    Api
}

public enum RegularizationStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Approve,
    Reject,
    Login,
    Export,
    Reopen
}
