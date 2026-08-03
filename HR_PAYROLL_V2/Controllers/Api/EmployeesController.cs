using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers.Api;

/// <summary>Back-office employee directory endpoints for HR/admin integrations.</summary>
[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class EmployeesController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll([FromQuery] string? q)
    {
        var query = _unitOfWork.Employees.Query()
            .Include(e => e.Company)
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.Designation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(e => e.FirstName.Contains(q) || e.LastName.Contains(q) || e.EmployeeCode.Contains(q));
        }

        var employees = await query.OrderBy(e => e.EmployeeCode).ToListAsync();
        return Ok(employees.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.Company)
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(ToDto(employee));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeDto>> Create(EmployeeCreateRequest request)
    {
        if (await _unitOfWork.Employees.ExistsAsync(e => e.EmployeeCode == request.EmployeeCode))
        {
            ModelState.AddModelError(nameof(request.EmployeeCode), "This employee ID is already in use.");
            return ValidationProblem(ModelState);
        }

        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            CompanyId = request.CompanyId,
            OrganizationalUnitId = request.OrganizationalUnitId,
            DesignationId = request.DesignationId,
            GradeId = request.GradeId,
            EmploymentTypeId = request.EmploymentTypeId,
            EmployeeCategoryId = request.EmployeeCategoryId,
            DateOfJoining = request.DateOfJoining,
            Status = request.Status,
            WorkLocation = request.WorkLocation
        };

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ToDto(employee));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, EmployeeUpdateRequest request)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (await _unitOfWork.Employees.ExistsAsync(e => e.EmployeeCode == request.EmployeeCode && e.Id != id))
        {
            ModelState.AddModelError(nameof(request.EmployeeCode), "This employee ID is already in use.");
            return ValidationProblem(ModelState);
        }

        employee.EmployeeCode = request.EmployeeCode;
        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.CompanyId = request.CompanyId;
        employee.OrganizationalUnitId = request.OrganizationalUnitId;
        employee.DesignationId = request.DesignationId;
        employee.GradeId = request.GradeId;
        employee.EmploymentTypeId = request.EmploymentTypeId;
        employee.EmployeeCategoryId = request.EmployeeCategoryId;
        employee.DateOfJoining = request.DateOfJoining;
        employee.Status = request.Status;
        employee.WorkLocation = request.WorkLocation;

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ToDto(employee));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.IsDeleted = true;
        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static EmployeeDto ToDto(Employee e) => new()
    {
        Id = e.Id,
        EmployeeCode = e.EmployeeCode,
        FirstName = e.FirstName,
        LastName = e.LastName,
        FullName = e.FullName,
        Email = e.Email,
        Phone = e.Phone,
        CompanyId = e.CompanyId,
        CompanyName = e.Company?.Name,
        OrganizationalUnitId = e.OrganizationalUnitId,
        OrganizationalUnitName = e.OrganizationalUnit?.Name,
        DesignationId = e.DesignationId,
        DesignationTitle = e.Designation?.Title,
        DateOfJoining = e.DateOfJoining,
        Status = e.Status,
        WorkLocation = e.WorkLocation,
        PhotoUrl = e.PhotoUrl
    };
}
