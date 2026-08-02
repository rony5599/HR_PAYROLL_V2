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
public class SalaryComponentController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyLookupService _companyLookup;

    public SalaryComponentController(IUnitOfWork unitOfWork, ICompanyLookupService companyLookup)
    {
        _unitOfWork = unitOfWork;
        _companyLookup = companyLookup;
    }

    public async Task<IActionResult> Index()
    {
        var components = await _unitOfWork.SalaryComponents.Query().Include(c => c.Company).OrderBy(c => c.Type).ThenBy(c => c.Name).ToListAsync();
        return View(components);
    }

    private async Task PopulateDropdownsAsync(Guid? companyId = null)
    {
        ViewBag.Companies = new SelectList(await _companyLookup.GetAllAsync(), "Id", "Name", companyId);
    }

    public async Task<IActionResult> Create()
    {
        var model = new SalaryComponentViewModel();
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
    public async Task<IActionResult> Create(SalaryComponentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        await _unitOfWork.SalaryComponents.AddAsync(new SalaryComponent
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Code = model.Code,
            Type = model.Type,
            Method = model.Method,
            Value = model.Value,
            IsTaxable = model.IsTaxable,
            IsActive = model.IsActive
        });
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Salary component saved.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var component = await _unitOfWork.SalaryComponents.GetByIdAsync(id);
        if (component is null)
        {
            return NotFound();
        }

        var model = new SalaryComponentViewModel
        {
            Id = component.Id,
            CompanyId = component.CompanyId,
            Name = component.Name,
            Code = component.Code,
            Type = component.Type,
            Method = component.Method,
            Value = component.Value,
            IsTaxable = component.IsTaxable,
            IsActive = component.IsActive
        };

        await PopulateDropdownsAsync(model.CompanyId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SalaryComponentViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model.CompanyId);
            return View(model);
        }

        var component = await _unitOfWork.SalaryComponents.GetByIdAsync(id);
        if (component is null)
        {
            return NotFound();
        }

        component.CompanyId = model.CompanyId;
        component.Name = model.Name;
        component.Code = model.Code;
        component.Type = model.Type;
        component.Method = model.Method;
        component.Value = model.Value;
        component.IsTaxable = model.IsTaxable;
        component.IsActive = model.IsActive;

        _unitOfWork.SalaryComponents.Update(component);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Salary component updated.";
        return RedirectToAction(nameof(Index));
    }
}
