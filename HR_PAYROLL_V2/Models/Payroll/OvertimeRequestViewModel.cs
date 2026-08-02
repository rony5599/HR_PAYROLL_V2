using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Payroll;

public class OvertimeRequestViewModel
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly OvertimeDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, Range(0.25, 24)]
    public decimal Hours { get; set; }

    [Required, StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class OvertimeDecisionViewModel
{
    public Guid Id { get; set; }
    public string? Comment { get; set; }
}
