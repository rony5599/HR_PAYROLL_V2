using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers.Api;

/// <summary>Organizational unit (department) directory for HR/admin integrations.</summary>
[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class DepartmentsController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll()
    {
        var units = await _unitOfWork.OrganizationalUnits.GetAllAsync();
        return Ok(units.OrderBy(u => u.Name).Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id)
    {
        var unit = await _unitOfWork.OrganizationalUnits.GetByIdAsync(id);
        return unit is null ? NotFound() : Ok(ToDto(unit));
    }

    [HttpPost]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DepartmentDto>> Create(DepartmentCreateRequest request)
    {
        if (await _unitOfWork.OrganizationalUnits.ExistsAsync(u => u.Code == request.Code && u.CompanyId == request.CompanyId))
        {
            ModelState.AddModelError(nameof(request.Code), "This department code is already in use for the company.");
            return ValidationProblem(ModelState);
        }

        var unit = new OrganizationalUnit
        {
            Name = request.Name,
            Code = request.Code,
            Level = request.Level,
            CompanyId = request.CompanyId,
            ParentUnitId = request.ParentUnitId,
            IsActive = true
        };

        await _unitOfWork.OrganizationalUnits.AddAsync(unit);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = unit.Id }, ToDto(unit));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DepartmentDto>> Update(Guid id, DepartmentUpdateRequest request)
    {
        var unit = await _unitOfWork.OrganizationalUnits.GetByIdAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        if (await _unitOfWork.OrganizationalUnits.ExistsAsync(u => u.Code == request.Code && u.CompanyId == request.CompanyId && u.Id != id))
        {
            ModelState.AddModelError(nameof(request.Code), "This department code is already in use for the company.");
            return ValidationProblem(ModelState);
        }

        unit.Name = request.Name;
        unit.Code = request.Code;
        unit.Level = request.Level;
        unit.CompanyId = request.CompanyId;
        unit.ParentUnitId = request.ParentUnitId;
        unit.IsActive = request.IsActive;

        _unitOfWork.OrganizationalUnits.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ToDto(unit));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var unit = await _unitOfWork.OrganizationalUnits.GetByIdAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        unit.IsDeleted = true;
        _unitOfWork.OrganizationalUnits.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static DepartmentDto ToDto(OrganizationalUnit u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Code = u.Code,
        Level = u.Level,
        CompanyId = u.CompanyId,
        ParentUnitId = u.ParentUnitId,
        IsActive = u.IsActive
    };
}
