using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Models.Department;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class DepartmentController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var units = await _unitOfWork.OrganizationalUnits.Query().Include(u => u.Company).ToListAsync();
        return View(units);
    }

    private async Task PopulateDropdownsAsync(DepartmentViewModel? model = null)
    {
        ViewBag.Companies = new SelectList(await _unitOfWork.Companies.GetAllAsync(), "Id", "Name", model?.CompanyId);
        var parents = model?.CompanyId is Guid companyId
            ? await _unitOfWork.OrganizationalUnits.FindAsync(o => o.CompanyId == companyId && o.Id != model.Id)
            : await _unitOfWork.OrganizationalUnits.GetAllAsync();
        ViewBag.ParentUnits = new SelectList(parents, "Id", "Name", model?.ParentUnitId);
    }

    public async Task<IActionResult> Create()
    {
        var model = new DepartmentViewModel();
        var firstCompany = (await _unitOfWork.Companies.GetAllAsync()).FirstOrDefault();
        if (firstCompany is not null)
        {
            model.CompanyId = firstCompany.Id;
        }

        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var unit = new OrganizationalUnit
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Code = model.Code,
            Level = model.Level,
            ParentUnitId = model.ParentUnitId,
            HeadEmployeeId = model.HeadEmployeeId,
            CostCenter = model.CostCenter,
            Description = model.Description,
            IsActive = model.IsActive
        };

        await _unitOfWork.OrganizationalUnits.AddAsync(unit);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Department saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var unit = await _unitOfWork.OrganizationalUnits.GetByIdAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        var model = new DepartmentViewModel
        {
            Id = unit.Id,
            CompanyId = unit.CompanyId,
            Name = unit.Name,
            Code = unit.Code,
            Level = unit.Level,
            ParentUnitId = unit.ParentUnitId,
            HeadEmployeeId = unit.HeadEmployeeId,
            CostCenter = unit.CostCenter,
            Description = unit.Description,
            IsActive = unit.IsActive
        };

        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DepartmentViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var unit = await _unitOfWork.OrganizationalUnits.GetByIdAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        unit.CompanyId = model.CompanyId;
        unit.Name = model.Name;
        unit.Code = model.Code;
        unit.Level = model.Level;
        unit.ParentUnitId = model.ParentUnitId == id ? null : model.ParentUnitId;
        unit.HeadEmployeeId = model.HeadEmployeeId;
        unit.CostCenter = model.CostCenter;
        unit.Description = model.Description;
        unit.IsActive = model.IsActive;

        _unitOfWork.OrganizationalUnits.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Department updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var unit = await _unitOfWork.OrganizationalUnits.GetByIdAsync(id);
        if (unit is not null)
        {
            unit.IsDeleted = true;
            _unitOfWork.OrganizationalUnits.Update(unit);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Department removed.";
        }

        return RedirectToAction(nameof(Index));
    }
}
