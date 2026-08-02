using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Caching;
using HR_PAYROLL_V2.Models.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_PAYROLL_V2.Controllers;

[Authorize(Roles = "SuperAdministrator,CompanyAdministrator,HRAdministrator")]
public class MasterDataController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyLookupService _companyLookup;

    public MasterDataController(IUnitOfWork unitOfWork, ICompanyLookupService companyLookup)
    {
        _unitOfWork = unitOfWork;
        _companyLookup = companyLookup;
    }

    public async Task<IActionResult> Index(Guid? companyId)
    {
        var companies = await _companyLookup.GetAllAsync();
        var effectiveCompanyId = companyId ?? companies.FirstOrDefault()?.Id;

        var model = new MasterDataPageViewModel
        {
            SelectedCompanyId = effectiveCompanyId,
            Companies = companies,
            Designations = effectiveCompanyId is Guid c1 ? await _unitOfWork.Designations.FindAsync(d => d.CompanyId == c1) : Array.Empty<Designation>(),
            Grades = effectiveCompanyId is Guid c2 ? await _unitOfWork.Grades.FindAsync(g => g.CompanyId == c2) : Array.Empty<Grade>(),
            EmploymentTypes = effectiveCompanyId is Guid c3 ? await _unitOfWork.EmploymentTypes.FindAsync(e => e.CompanyId == c3) : Array.Empty<EmploymentType>(),
            EmployeeCategories = effectiveCompanyId is Guid c4 ? await _unitOfWork.EmployeeCategories.FindAsync(e => e.CompanyId == c4) : Array.Empty<EmployeeCategory>()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDesignation(MasterDataItemViewModel model)
    {
        await _unitOfWork.Designations.AddAsync(new Designation { CompanyId = model.CompanyId, Title = model.Name, Code = model.Code });
        await _unitOfWork.SaveChangesAsync();
        TempData["Success"] = "Designation added.";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGrade(MasterDataItemViewModel model)
    {
        await _unitOfWork.Grades.AddAsync(new Grade { CompanyId = model.CompanyId, Name = model.Name, Code = model.Code, Level = model.Level ?? 0 });
        await _unitOfWork.SaveChangesAsync();
        TempData["Success"] = "Grade added.";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEmploymentType(MasterDataItemViewModel model)
    {
        await _unitOfWork.EmploymentTypes.AddAsync(new EmploymentType { CompanyId = model.CompanyId, Name = model.Name, Code = model.Code });
        await _unitOfWork.SaveChangesAsync();
        TempData["Success"] = "Employment type added.";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEmployeeCategory(MasterDataItemViewModel model)
    {
        await _unitOfWork.EmployeeCategories.AddAsync(new EmployeeCategory { CompanyId = model.CompanyId, Name = model.Name, Code = model.Code });
        await _unitOfWork.SaveChangesAsync();
        TempData["Success"] = "Employee category added.";
        return RedirectToAction(nameof(Index), new { companyId = model.CompanyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string type, Guid id, Guid companyId)
    {
        switch (type)
        {
            case "designation":
                var designation = await _unitOfWork.Designations.GetByIdAsync(id);
                if (designation is not null) { designation.IsActive = !designation.IsActive; _unitOfWork.Designations.Update(designation); }
                break;
            case "grade":
                var grade = await _unitOfWork.Grades.GetByIdAsync(id);
                if (grade is not null) { grade.IsActive = !grade.IsActive; _unitOfWork.Grades.Update(grade); }
                break;
            case "employmenttype":
                var employmentType = await _unitOfWork.EmploymentTypes.GetByIdAsync(id);
                if (employmentType is not null) { employmentType.IsActive = !employmentType.IsActive; _unitOfWork.EmploymentTypes.Update(employmentType); }
                break;
            case "employeecategory":
                var category = await _unitOfWork.EmployeeCategories.GetByIdAsync(id);
                if (category is not null) { category.IsActive = !category.IsActive; _unitOfWork.EmployeeCategories.Update(category); }
                break;
        }

        await _unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { companyId });
    }
}
