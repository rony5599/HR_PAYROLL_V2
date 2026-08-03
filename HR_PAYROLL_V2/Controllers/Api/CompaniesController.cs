using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers.Api;

/// <summary>Company directory for admin/integration clients.</summary>
[Authorize(Roles = "SuperAdministrator,CompanyAdministrator")]
public class CompaniesController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CompaniesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetAll()
    {
        var companies = await _unitOfWork.Companies.GetAllAsync();
        return Ok(companies.OrderBy(c => c.Name).Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDto>> GetById(Guid id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        return company is null ? NotFound() : Ok(ToDto(company));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdministrator")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyDto>> Create(CompanyCreateRequest request)
    {
        if (await _unitOfWork.Companies.ExistsAsync(c => c.Code == request.Code))
        {
            ModelState.AddModelError(nameof(request.Code), "This company code is already in use.");
            return ValidationProblem(ModelState);
        }

        var company = new Company
        {
            Name = request.Name,
            Code = request.Code,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            PayrollCurrency = request.PayrollCurrency,
            PayrollFrequency = request.PayrollFrequency,
            IsActive = true
        };

        await _unitOfWork.Companies.AddAsync(company);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = company.Id }, ToDto(company));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdministrator")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, CompanyUpdateRequest request)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is null)
        {
            return NotFound();
        }

        if (await _unitOfWork.Companies.ExistsAsync(c => c.Code == request.Code && c.Id != id))
        {
            ModelState.AddModelError(nameof(request.Code), "This company code is already in use.");
            return ValidationProblem(ModelState);
        }

        company.Name = request.Name;
        company.Code = request.Code;
        company.ContactEmail = request.ContactEmail;
        company.ContactPhone = request.ContactPhone;
        company.PayrollCurrency = request.PayrollCurrency;
        company.PayrollFrequency = request.PayrollFrequency;
        company.IsActive = request.IsActive;

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ToDto(company));
    }

    private static CompanyDto ToDto(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Code = c.Code,
        ContactEmail = c.ContactEmail,
        ContactPhone = c.ContactPhone,
        PayrollCurrency = c.PayrollCurrency,
        PayrollFrequency = c.PayrollFrequency,
        IsActive = c.IsActive
    };
}
