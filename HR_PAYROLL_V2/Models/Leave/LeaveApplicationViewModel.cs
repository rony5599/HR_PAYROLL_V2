using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Leave;

public class LeaveApplicationViewModel
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    public Guid LeaveTypeId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, DataType(DataType.Date)]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class LeaveDecisionViewModel
{
    public Guid Id { get; set; }
    public string? Comment { get; set; }
}
