using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Authorization;
using HR_PAYROLL_V2.Infrastructure.Caching;
using HR_PAYROLL_V2.Infrastructure.Identity;
using HR_PAYROLL_V2.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyLookupService _companyLookup;
    private readonly IPasswordHasher _passwordHasher;

    public UserController(IUnitOfWork unitOfWork, ICompanyLookupService companyLookup, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _companyLookup = companyLookup;
        _passwordHasher = passwordHasher;
    }

    [RequirePermission("Users.View", "Users.Manage")]
    public async Task<IActionResult> Index()
    {
        var users = await _unitOfWork.Users.Query().Include(u => u.Company).Include(u => u.Employee).ToListAsync();
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roles = (await _unitOfWork.Roles.GetAllAsync()).ToDictionary(r => r.Id, r => r.Name);

        ViewBag.RoleNamesByUser = userRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.Select(ur => roles.GetValueOrDefault(ur.RoleId, "?")).ToList());

        return View(users);
    }

    private async Task PopulateDropdownsAsync(UserViewModel? model = null, Guid? currentUserId = null)
    {
        ViewBag.Companies = new SelectList(await _companyLookup.GetAllAsync(), "Id", "Name", model?.CompanyId);
        ViewBag.Roles = await _unitOfWork.Roles.GetAllAsync();

        var linkableEmployees = (await _unitOfWork.Employees.Query()
            .Where(e => e.UserId == null || e.UserId == currentUserId)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync());
        ViewBag.Employees = new SelectList(linkableEmployees, "Id", "FullName", model?.EmployeeId);
    }

    [RequirePermission("Users.Manage")]
    public async Task<IActionResult> Create()
    {
        var model = new UserViewModel();
        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users.Manage")]
    public async Task<IActionResult> Create(UserViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        }

        if (await _unitOfWork.Users.ExistsAsync(u => u.Username == model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
        }

        if (await _unitOfWork.Users.ExistsAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
        }

        Employee? employeeToLink = null;
        if (model.EmployeeId.HasValue)
        {
            employeeToLink = await _unitOfWork.Employees.GetByIdAsync(model.EmployeeId.Value);
            if (employeeToLink is null)
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Selected employee was not found.");
            }
            else if (employeeToLink.UserId.HasValue)
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "This employee is already linked to another user account.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var user = new Domain.Entities.User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = _passwordHasher.Hash(model.Password!),
            CompanyId = model.CompanyId,
            IsActive = model.IsActive
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        foreach (var roleId in model.SelectedRoleIds)
        {
            await _unitOfWork.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = roleId });
        }

        if (employeeToLink is not null)
        {
            employeeToLink.UserId = user.Id;
            _unitOfWork.Employees.Update(employeeToLink);
        }

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("Users.Manage")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var assignedRoleIds = (await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == id))
            .Select(ur => ur.RoleId)
            .ToList();

        var linkedEmployee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == id)).FirstOrDefault();

        var model = new UserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CompanyId = user.CompanyId,
            EmployeeId = linkedEmployee?.Id,
            IsActive = user.IsActive,
            SelectedRoleIds = assignedRoleIds
        };

        await PopulateDropdownsAsync(model, currentUserId: id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users.Manage")]
    public async Task<IActionResult> Edit(Guid id, UserViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (await _unitOfWork.Users.ExistsAsync(u => u.Username == model.Username && u.Id != id))
        {
            ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
        }

        if (await _unitOfWork.Users.ExistsAsync(u => u.Email == model.Email && u.Id != id))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
        }

        Employee? employeeToLink = null;
        if (model.EmployeeId.HasValue)
        {
            employeeToLink = await _unitOfWork.Employees.GetByIdAsync(model.EmployeeId.Value);
            if (employeeToLink is null)
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Selected employee was not found.");
            }
            else if (employeeToLink.UserId.HasValue && employeeToLink.UserId != id)
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "This employee is already linked to another user account.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, currentUserId: id);
            return View(model);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.Username = model.Username;
        user.Email = model.Email;
        user.CompanyId = model.CompanyId;
        user.IsActive = model.IsActive;
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(model.Password);
        }

        _unitOfWork.Users.Update(user);

        var previouslyLinkedEmployee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == id)).FirstOrDefault();
        if (previouslyLinkedEmployee is not null && previouslyLinkedEmployee.Id != employeeToLink?.Id)
        {
            previouslyLinkedEmployee.UserId = null;
            _unitOfWork.Employees.Update(previouslyLinkedEmployee);
        }

        if (employeeToLink is not null)
        {
            employeeToLink.UserId = id;
            _unitOfWork.Employees.Update(employeeToLink);
        }

        var existingRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == id);
        foreach (var userRole in existingRoles.Where(ur => !model.SelectedRoleIds.Contains(ur.RoleId)))
        {
            _unitOfWork.UserRoles.Remove(userRole);
        }

        foreach (var roleId in model.SelectedRoleIds.Except(existingRoles.Select(ur => ur.RoleId)))
        {
            await _unitOfWork.UserRoles.AddAsync(new UserRole { UserId = id, RoleId = roleId });
        }

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users.Manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is not null)
        {
            user.IsDeleted = true;
            user.IsActive = false;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "User removed.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users.Manage")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is not null)
        {
            user.IsActive = !user.IsActive;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = user.IsActive ? "User activated." : "User deactivated.";
        }

        return RedirectToAction(nameof(Index));
    }
}
