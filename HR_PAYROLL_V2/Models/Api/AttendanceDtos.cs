using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Api;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public decimal WorkedHours { get; set; }
    public AttendanceStatus Status { get; set; }
    public AttendanceSource Source { get; set; }
    public string? Remarks { get; set; }
}

public class CheckInRequest
{
    public string? Remarks { get; set; }
}

public class CheckOutRequest
{
    public string? Remarks { get; set; }
}
