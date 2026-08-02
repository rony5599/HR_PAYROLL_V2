using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Caching;
using HR_PAYROLL_V2.Models.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class SalaryStructureController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyLookupService _companyLookup;

    public SalaryStructureController(IUnitOfWork unitOfWork, ICompanyLookupService companyLookup)
    {
        _unitOfWork = unitOfWork;
        _companyLookup = companyLookup;
    }

    public async Task<IActionResult> Index()
    {
        var structures = await _unitOfWork.SalaryStructures.Query()
            .Include(s => s.Company)
            .Include(s => s.Components).ThenInclude(c => c.SalaryComponent)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(structures);
    }

    private async Task PopulateDropdownsAsync(Guid? companyId = null, List<Guid>? selected = null)
    {
        ViewBag.Companies = new SelectList(await _companyLookup.GetAllAsync(), "Id", "Name", companyId);
        var components = companyId is Guid cid
            ? await _unitOfWork.SalaryComponents.FindAsync(c => c.CompanyId == cid && c.IsActive)
            : Array.Empty<SalaryComponent>();
        ViewBag.Components = components;
        ViewBag.SelectedComponentIds = selected ?? new List<Guid>();
    }

    public async Task<IActionResult> Create()
    {
        var model = new SalaryStructureViewModel();
        var firstCompany = (await _companyLookup.GetAllAsync()).FirstOrDefault();
        if (firstCompany is not null)
        {
            model.CompanyId = firstCompany.Id;
        }

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SalaryStructureViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId, model.SelectedComponentIds);
            return View(model);
        }

        var structure = new SalaryStructure
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Code = model.Code,
            PayrollFrequency = model.PayrollFrequency,
            ProrationMethod = model.ProrationMethod,
            EffectiveDate = model.EffectiveDate,
            IsActive = model.IsActive
        };

        await _unitOfWork.SalaryStructures.AddAsync(structure);
        await _unitOfWork.SaveChangesAsync();

        foreach (var componentId in model.SelectedComponentIds.Distinct())
        {
            await _unitOfWork.SalaryStructureComponents.AddAsync(new SalaryStructureComponent
            {
                SalaryStructureId = structure.Id,
                SalaryComponentId = componentId
            });
        }
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Salary structure saved.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var structure = await _unitOfWork.SalaryStructures.Query()
            .Include(s => s.Components)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (structure is null)
        {
            return NotFound();
        }

        var model = new SalaryStructureViewModel
        {
            Id = structure.Id,
            CompanyId = structure.CompanyId,
            Name = structure.Name,
            Code = structure.Code,
            PayrollFrequency = structure.PayrollFrequency,
            ProrationMethod = structure.ProrationMethod,
            EffectiveDate = structure.EffectiveDate,
            IsActive = structure.IsActive,
            SelectedComponentIds = structure.Components.Select(c => c.SalaryComponentId).ToList()
        };

        await PopulateDropdownsAsync(model.CompanyId, model.SelectedComponentIds);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SalaryStructureViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId, model.SelectedComponentIds);
            return View(model);
        }

        var structure = await _unitOfWork.SalaryStructures.Query()
            .Include(s => s.Components)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (structure is null)
        {
            return NotFound();
        }

        structure.CompanyId = model.CompanyId;
        structure.Name = model.Name;
        structure.Code = model.Code;
        structure.PayrollFrequency = model.PayrollFrequency;
        structure.ProrationMethod = model.ProrationMethod;
        structure.EffectiveDate = model.EffectiveDate;
        structure.IsActive = model.IsActive;
        _unitOfWork.SalaryStructures.Update(structure);

        var selected = model.SelectedComponentIds.Distinct().ToHashSet();
        var existing = structure.Components.Select(c => c.SalaryComponentId).ToHashSet();

        foreach (var toRemove in structure.Components.Where(c => !selected.Contains(c.SalaryComponentId)).ToList())
        {
            _unitOfWork.SalaryStructureComponents.Remove(toRemove);
        }

        foreach (var toAdd in selected.Where(id => !existing.Contains(id)))
        {
            await _unitOfWork.SalaryStructureComponents.AddAsync(new SalaryStructureComponent
            {
                SalaryStructureId = structure.Id,
                SalaryComponentId = toAdd
            });
        }

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Salary structure updated.";
        return RedirectToAction(nameof(Index));
    }
}
