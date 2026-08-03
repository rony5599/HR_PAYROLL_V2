using System.ComponentModel.DataAnnotations;

namespace HR_PAYROLL_V2.Models.Api;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string PayrollCurrency { get; set; } = string.Empty;
    public string PayrollFrequency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CompanyCreateRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [EmailAddress]
    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }
    public string PayrollCurrency { get; set; } = "USD";
    public string PayrollFrequency { get; set; } = "Monthly";
}

public class CompanyUpdateRequest : CompanyCreateRequest
{
    public bool IsActive { get; set; } = true;
}
