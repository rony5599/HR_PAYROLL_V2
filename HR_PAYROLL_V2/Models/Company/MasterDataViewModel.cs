using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Company;

public class MasterDataItemViewModel
{
    [Required]
    public Guid CompanyId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    public int? Level { get; set; }
}

public class MasterDataPageViewModel
{
    public Guid? SelectedCompanyId { get; set; }
    public IReadOnlyList<Domain.Entities.Company> Companies { get; set; } = Array.Empty<Domain.Entities.Company>();
    public IReadOnlyList<Domain.Entities.Designation> Designations { get; set; } = Array.Empty<Domain.Entities.Designation>();
    public IReadOnlyList<Domain.Entities.Grade> Grades { get; set; } = Array.Empty<Domain.Entities.Grade>();
    public IReadOnlyList<Domain.Entities.EmploymentType> EmploymentTypes { get; set; } = Array.Empty<Domain.Entities.EmploymentType>();
    public IReadOnlyList<Domain.Entities.EmployeeCategory> EmployeeCategories { get; set; } = Array.Empty<Domain.Entities.EmployeeCategory>();
}
