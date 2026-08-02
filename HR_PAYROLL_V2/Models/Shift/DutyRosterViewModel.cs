using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Shift;

public class DutyRosterViewModel
{
    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, DataType(DataType.Date)]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(6));

    [Required]
    public Guid ShiftId { get; set; }

    public Guid? OrganizationalUnitId { get; set; }

    public List<Guid> SelectedEmployeeIds { get; set; } = new();
}
