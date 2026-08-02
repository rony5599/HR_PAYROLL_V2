using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Caching;
using HR_PAYROLL_V2.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class EmployeeController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyLookupService _companyLookup;
    private readonly ICacheService _cache;

    public EmployeeController(IUnitOfWork unitOfWork, ICompanyLookupService companyLookup, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _companyLookup = companyLookup;
        _cache = cache;
    }

    private async Task InvalidateSummaryCachesAsync()
    {
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("organogram:tree");
    }

    public async Task<IActionResult> Index(string? q)
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

        ViewBag.Query = q;
        return View(await query.OrderBy(e => e.EmployeeCode).ToListAsync());
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.Company)
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.Designation)
            .Include(e => e.Grade)
            .Include(e => e.EmploymentType)
            .Include(e => e.EmployeeCategory)
            .Include(e => e.ReportingRelationships).ThenInclude(r => r.Manager)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        ViewBag.CurrentShift = (await _unitOfWork.ShiftAssignments.Query()
            .Include(a => a.Shift)
            .Where(a => a.EmployeeId == id && a.IsActive)
            .ToListAsync()).FirstOrDefault()?.Shift;

        return View(employee);
    }

    private async Task PopulateDropdownsAsync(EmployeeViewModel? model = null)
    {
        ViewBag.Companies = new SelectList(await _companyLookup.GetAllAsync(), "Id", "Name", model?.CompanyId);
        ViewBag.Departments = new SelectList(await _unitOfWork.OrganizationalUnits.GetAllAsync(), "Id", "Name", model?.OrganizationalUnitId);
        ViewBag.Designations = new SelectList(await _unitOfWork.Designations.GetAllAsync(), "Id", "Title", model?.DesignationId);
        ViewBag.Grades = new SelectList(await _unitOfWork.Grades.GetAllAsync(), "Id", "Name", model?.GradeId);
        ViewBag.EmploymentTypes = new SelectList(await _unitOfWork.EmploymentTypes.GetAllAsync(), "Id", "Name", model?.EmploymentTypeId);
        ViewBag.EmployeeCategories = new SelectList(await _unitOfWork.EmployeeCategories.GetAllAsync(), "Id", "Name", model?.EmployeeCategoryId);

        var managers = await _unitOfWork.Employees.GetAllAsync();
        var managerOptions = model?.Id is Guid selfId ? managers.Where(m => m.Id != selfId) : managers;
        ViewBag.Managers = new SelectList(managerOptions, "Id", "FullName", model?.ReportingManagerId);

        ViewBag.Shifts = new SelectList(await _unitOfWork.Shifts.FindAsync(s => s.IsActive), "Id", "Name", model?.ShiftId);
        ViewBag.SalaryStructures = new SelectList(await _unitOfWork.SalaryStructures.FindAsync(s => s.IsActive), "Id", "Name", model?.SalaryStructureId);
    }

    public async Task<IActionResult> Create()
    {
        var model = new EmployeeViewModel();
        var firstCompany = (await _companyLookup.GetAllAsync()).FirstOrDefault();
        if (firstCompany is not null)
        {
            model.CompanyId = firstCompany.Id;
        }

        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeViewModel model)
    {
        if (model.ReportingManagerId.HasValue && model.Id == model.ReportingManagerId)
        {
            ModelState.AddModelError(nameof(model.ReportingManagerId), "An employee cannot report to themselves.");
        }

        if (await _unitOfWork.Employees.ExistsAsync(e => e.EmployeeCode == model.EmployeeCode))
        {
            ModelState.AddModelError(nameof(model.EmployeeCode), "This employee ID is already in use.");
        }

        if (model.SalaryStructureId.HasValue && !model.BasicSalary.HasValue)
        {
            ModelState.AddModelError(nameof(model.BasicSalary), "Basic salary is required when a salary structure is assigned.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var employee = new Employee
        {
            EmployeeCode = model.EmployeeCode,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyId = model.CompanyId,
            OrganizationalUnitId = model.OrganizationalUnitId,
            DesignationId = model.DesignationId,
            GradeId = model.GradeId,
            EmploymentTypeId = model.EmploymentTypeId,
            EmployeeCategoryId = model.EmployeeCategoryId,
            DateOfJoining = model.DateOfJoining,
            Status = model.Status,
            WorkLocation = model.WorkLocation
        };

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        if (model.ReportingManagerId.HasValue)
        {
            await _unitOfWork.ReportingRelationships.AddAsync(new EmployeeReportingManager
            {
                EmployeeId = employee.Id,
                ManagerId = model.ReportingManagerId.Value,
                RelationshipType = ReportingRelationshipType.Primary,
                EffectiveStartDate = model.DateOfJoining,
                IsActive = true
            });
        }

        if (model.ShiftId.HasValue)
        {
            await _unitOfWork.ShiftAssignments.AddAsync(new ShiftAssignment
            {
                EmployeeId = employee.Id,
                ShiftId = model.ShiftId.Value,
                EffectiveStartDate = model.DateOfJoining,
                IsActive = true
            });
        }

        if (model.SalaryStructureId.HasValue && model.BasicSalary.HasValue)
        {
            await _unitOfWork.EmployeeSalaryAssignments.AddAsync(new EmployeeSalaryAssignment
            {
                EmployeeId = employee.Id,
                SalaryStructureId = model.SalaryStructureId.Value,
                BasicSalary = model.BasicSalary.Value,
                EffectiveStartDate = model.DateOfJoining,
                IsActive = true
            });
        }

        await _unitOfWork.SaveChangesAsync();
        await InvalidateSummaryCachesAsync();

        TempData["Success"] = "Employee created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        var activeManager = (await _unitOfWork.ReportingRelationships.FindAsync(
            r => r.EmployeeId == id && r.IsActive && r.RelationshipType == ReportingRelationshipType.Primary)).FirstOrDefault();

        var model = new EmployeeViewModel
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            CompanyId = employee.CompanyId,
            OrganizationalUnitId = employee.OrganizationalUnitId,
            DesignationId = employee.DesignationId,
            GradeId = employee.GradeId,
            EmploymentTypeId = employee.EmploymentTypeId,
            EmployeeCategoryId = employee.EmployeeCategoryId,
            DateOfJoining = employee.DateOfJoining,
            Status = employee.Status,
            WorkLocation = employee.WorkLocation,
            ReportingManagerId = activeManager?.ManagerId
        };

        var activeShift = (await _unitOfWork.ShiftAssignments.FindAsync(a => a.EmployeeId == id && a.IsActive)).FirstOrDefault();
        model.ShiftId = activeShift?.ShiftId;

        var activeSalary = (await _unitOfWork.EmployeeSalaryAssignments.FindAsync(a => a.EmployeeId == id && a.IsActive)).FirstOrDefault();
        model.SalaryStructureId = activeSalary?.SalaryStructureId;
        model.BasicSalary = activeSalary?.BasicSalary;

        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EmployeeViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (model.ReportingManagerId == id)
        {
            ModelState.AddModelError(nameof(model.ReportingManagerId), "An employee cannot report to themselves.");
        }

        if (await _unitOfWork.Employees.ExistsAsync(e => e.EmployeeCode == model.EmployeeCode && e.Id != id))
        {
            ModelState.AddModelError(nameof(model.EmployeeCode), "This employee ID is already in use.");
        }

        if (model.SalaryStructureId.HasValue && !model.BasicSalary.HasValue)
        {
            ModelState.AddModelError(nameof(model.BasicSalary), "Basic salary is required when a salary structure is assigned.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.EmployeeCode = model.EmployeeCode;
        employee.FirstName = model.FirstName;
        employee.LastName = model.LastName;
        employee.Email = model.Email;
        employee.Phone = model.Phone;
        employee.CompanyId = model.CompanyId;
        employee.OrganizationalUnitId = model.OrganizationalUnitId;
        employee.DesignationId = model.DesignationId;
        employee.GradeId = model.GradeId;
        employee.EmploymentTypeId = model.EmploymentTypeId;
        employee.EmployeeCategoryId = model.EmployeeCategoryId;
        employee.DateOfJoining = model.DateOfJoining;
        employee.Status = model.Status;
        employee.WorkLocation = model.WorkLocation;

        _unitOfWork.Employees.Update(employee);

        var activeManager = (await _unitOfWork.ReportingRelationships.FindAsync(
            r => r.EmployeeId == id && r.IsActive && r.RelationshipType == ReportingRelationshipType.Primary)).FirstOrDefault();

        if (activeManager?.ManagerId != model.ReportingManagerId)
        {
            if (activeManager is not null)
            {
                activeManager.IsActive = false;
                activeManager.EffectiveEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                _unitOfWork.ReportingRelationships.Update(activeManager);
            }

            if (model.ReportingManagerId.HasValue)
            {
                await _unitOfWork.ReportingRelationships.AddAsync(new EmployeeReportingManager
                {
                    EmployeeId = id,
                    ManagerId = model.ReportingManagerId.Value,
                    RelationshipType = ReportingRelationshipType.Primary,
                    EffectiveStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsActive = true
                });
            }
        }

        var activeShift = (await _unitOfWork.ShiftAssignments.FindAsync(a => a.EmployeeId == id && a.IsActive)).FirstOrDefault();
        if (activeShift?.ShiftId != model.ShiftId)
        {
            if (activeShift is not null)
            {
                activeShift.IsActive = false;
                activeShift.EffectiveEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                _unitOfWork.ShiftAssignments.Update(activeShift);
            }

            if (model.ShiftId.HasValue)
            {
                await _unitOfWork.ShiftAssignments.AddAsync(new ShiftAssignment
                {
                    EmployeeId = id,
                    ShiftId = model.ShiftId.Value,
                    EffectiveStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsActive = true
                });
            }
        }

        var activeSalary = (await _unitOfWork.EmployeeSalaryAssignments.FindAsync(a => a.EmployeeId == id && a.IsActive)).FirstOrDefault();
        if (activeSalary?.SalaryStructureId != model.SalaryStructureId || activeSalary?.BasicSalary != model.BasicSalary)
        {
            if (activeSalary is not null)
            {
                activeSalary.IsActive = false;
                activeSalary.EffectiveEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                _unitOfWork.EmployeeSalaryAssignments.Update(activeSalary);
            }

            if (model.SalaryStructureId.HasValue && model.BasicSalary.HasValue)
            {
                await _unitOfWork.EmployeeSalaryAssignments.AddAsync(new EmployeeSalaryAssignment
                {
                    EmployeeId = id,
                    SalaryStructureId = model.SalaryStructureId.Value,
                    BasicSalary = model.BasicSalary.Value,
                    EffectiveStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsActive = true
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
        await InvalidateSummaryCachesAsync();

        TempData["Success"] = "Employee updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee is not null)
        {
            employee.IsDeleted = true;
            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync();
            await InvalidateSummaryCachesAsync();
            TempData["Success"] = "Employee removed.";
        }

        return RedirectToAction(nameof(Index));
    }
}
