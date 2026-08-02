using System.ComponentModel.DataAnnotations;
using HR_PAYROLL_V2.Domain.Enums;

namespace HR_PAYROLL_V2.Models.Company;

public class HolidayViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public HolidayType Type { get; set; } = HolidayType.PaidPublic;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
