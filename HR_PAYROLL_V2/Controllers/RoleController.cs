using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Authorization;
using HR_PAYROLL_V2.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Controllers;

[Authorize]
public class RoleController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [RequirePermission("Roles.View", "Roles.Manage")]
    public async Task<IActionResult> Index()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        ViewBag.PermissionCountByRole = rolePermissions
            .GroupBy(rp => rp.RoleId)
            .ToDictionary(g => g.Key, g => g.Count());

        return View(roles);
    }

    private async Task PopulatePermissionsAsync()
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        ViewBag.PermissionsByModule = permissions
            .GroupBy(p => p.Module)
            .OrderBy(g => g.Key)
            .ToList();
    }

    [RequirePermission("Roles.Manage")]
    public async Task<IActionResult> Create()
    {
        await PopulatePermissionsAsync();
        return View(new RoleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Roles.Manage")]
    public async Task<IActionResult> Create(RoleViewModel model)
    {
        if (await _unitOfWork.Roles.ExistsAsync(r => r.Name == model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            await PopulatePermissionsAsync();
            return View(model);
        }

        var role = new Role
        {
            Name = model.Name,
            Description = model.Description,
            IsSystemRole = false
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        foreach (var permissionId in model.SelectedPermissionIds)
        {
            await _unitOfWork.RolePermissions.AddAsync(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
        }

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Role created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("Roles.Manage")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var assignedPermissionIds = (await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == id))
            .Select(rp => rp.PermissionId)
            .ToList();

        var model = new RoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            SelectedPermissionIds = assignedPermissionIds
        };

        await PopulatePermissionsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Roles.Manage")]
    public async Task<IActionResult> Edit(Guid id, RoleViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (await _unitOfWork.Roles.ExistsAsync(r => r.Name == model.Name && r.Id != id))
        {
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            await PopulatePermissionsAsync();
            return View(model);
        }

        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        role.Name = role.IsSystemRole ? role.Name : model.Name;
        role.Description = model.Description;
        _unitOfWork.Roles.Update(role);

        var existingPermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == id);
        foreach (var rolePermission in existingPermissions.Where(rp => !model.SelectedPermissionIds.Contains(rp.PermissionId)))
        {
            _unitOfWork.RolePermissions.Remove(rolePermission);
        }

        foreach (var permissionId in model.SelectedPermissionIds.Except(existingPermissions.Select(rp => rp.PermissionId)))
        {
            await _unitOfWork.RolePermissions.AddAsync(new RolePermission { RoleId = id, PermissionId = permissionId });
        }

        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Role updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Roles.Manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role is not null)
        {
            if (role.IsSystemRole)
            {
                TempData["Success"] = "System roles cannot be removed.";
                return RedirectToAction(nameof(Index));
            }

            if (await _unitOfWork.UserRoles.ExistsAsync(ur => ur.RoleId == id))
            {
                TempData["Success"] = "This role is still assigned to users and cannot be removed.";
                return RedirectToAction(nameof(Index));
            }

            role.IsDeleted = true;
            _unitOfWork.Roles.Update(role);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Role removed.";
        }

        return RedirectToAction(nameof(Index));
    }
}
