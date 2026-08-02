using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Attendance;

public class ManualAttendanceViewModel
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly AttendanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Time)]
    public TimeOnly? CheckIn { get; set; }

    [DataType(DataType.Time)]
    public TimeOnly? CheckOut { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }
}

public class AttendanceRegularizationViewModel
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly AttendanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Time)]
    public TimeOnly? RequestedCheckIn { get; set; }

    [DataType(DataType.Time)]
    public TimeOnly? RequestedCheckOut { get; set; }

    [Required, StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class RegularizationDecisionViewModel
{
    public Guid Id { get; set; }
    public string? Comment { get; set; }
}
