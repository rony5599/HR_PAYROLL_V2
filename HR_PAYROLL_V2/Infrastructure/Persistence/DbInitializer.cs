using HR_PAYROLL_V2.Domain.Entities;
using HR_PAYROLL_V2.Domain.Enums;
using HR_PAYROLL_V2.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Name = "SuperAdministrator", Description = "Full system access", IsSystemRole = true },
                new Role { Name = "CompanyAdministrator", Description = "Manages a single company", IsSystemRole = true },
                new Role { Name = "HRAdministrator", Description = "Manages employees, org structure and policies", IsSystemRole = true },
                new Role { Name = "Employee", Description = "Standard employee self-service access", IsSystemRole = true }
            );
            await context.SaveChangesAsync();
        }

        var rolePermissionsExistedBeforeSeed = await context.RolePermissions.AnyAsync();

        var existingPermissionNames = (await context.Permissions.Select(p => p.Name).ToListAsync()).ToHashSet();
        var missingPermissions = PermissionCatalog.All().Where(p => !existingPermissionNames.Contains(p.Name)).ToList();
        if (missingPermissions.Count > 0)
        {
            context.Permissions.AddRange(missingPermissions.Select(p => new Permission
            {
                Name = p.Name,
                Module = p.Module,
                Description = p.Description
            }));
            await context.SaveChangesAsync();
        }

        if (!rolePermissionsExistedBeforeSeed)
        {
            var permissions = await context.Permissions.ToListAsync();
            var roles = await context.Roles.ToListAsync();

            var grants = new Dictionary<string, string[]>
            {
                ["SuperAdministrator"] = permissions.Select(p => p.Name).ToArray(),
                ["CompanyAdministrator"] = permissions.Select(p => p.Name)
                    .Where(n => n is not ("Roles.View" or "Roles.Manage" or "Users.Manage"))
                    .Append("Users.View")
                    .Distinct()
                    .ToArray(),
                ["HRAdministrator"] = new[]
                {
                    "Departments.View", "Departments.Manage",
                    "Employees.View", "Employees.Manage",
                    "Attendance.View", "Attendance.Manage",
                    "Leave.View", "Leave.Manage",
                    "Shifts.View", "Shifts.Manage",
                    "Holidays.View", "Holidays.Manage",
                    "Overtime.View", "Overtime.Manage",
                    "Payroll.View",
                    "Reports.View",
                    "AuditLogs.View"
                },
                ["Employee"] = Array.Empty<string>()
            };

            foreach (var role in roles)
            {
                if (!grants.TryGetValue(role.Name, out var permissionNames))
                {
                    continue;
                }

                foreach (var permissionName in permissionNames)
                {
                    var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                    if (permission is not null)
                    {
                        context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
        else if (missingPermissions.Any(p => p.Module == "AuditLogs"))
        {
            // Backfill audit-log permissions onto existing admin roles when upgrading a database
            // that was already seeded before the audit trail feature was added.
            var auditPermissions = await context.Permissions.Where(p => p.Module == "AuditLogs").ToListAsync();
            var roles = await context.Roles.ToListAsync();
            var existingGrants = (await context.RolePermissions.ToListAsync())
                .Select(rp => (rp.RoleId, rp.PermissionId))
                .ToHashSet();

            var auditGrants = new Dictionary<string, string[]>
            {
                ["SuperAdministrator"] = new[] { "AuditLogs.View", "AuditLogs.Manage" },
                ["CompanyAdministrator"] = new[] { "AuditLogs.View", "AuditLogs.Manage" },
                ["HRAdministrator"] = new[] { "AuditLogs.View" }
            };

            foreach (var role in roles)
            {
                if (!auditGrants.TryGetValue(role.Name, out var permissionNames))
                {
                    continue;
                }

                foreach (var permissionName in permissionNames)
                {
                    var permission = auditPermissions.FirstOrDefault(p => p.Name == permissionName);
                    if (permission is not null && !existingGrants.Contains((role.Id, permission.Id)))
                    {
                        context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        Company? company = await context.Companies.FirstOrDefaultAsync();
        if (company is null)
        {
            company = new Company
            {
                Name = "Nova Technologies Ltd.",
                Code = "NOVA-TECH",
                PayrollCurrency = "USD",
                PayrollFrequency = "Monthly",
                ContactEmail = "hr@novatech.example"
            };
            context.Companies.Add(company);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var superAdminRole = await context.Roles.FirstAsync(r => r.Name == "SuperAdministrator");
            var admin = new User
            {
                Username = "admin",
                Email = "admin@peopleflow.local",
                PasswordHash = passwordHasher.Hash("Admin@123"),
                CompanyId = company.Id,
                IsActive = true
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = superAdminRole.Id });
            await context.SaveChangesAsync();
        }

        if (!await context.OrganizationalUnits.AnyAsync())
        {
            context.OrganizationalUnits.AddRange(
                new OrganizationalUnit { CompanyId = company.Id, Name = "Human Resources", Code = "DEPT-HR", Level = OrganizationalUnitLevel.Department, IsActive = true },
                new OrganizationalUnit { CompanyId = company.Id, Name = "Software Engineering", Code = "DEPT-SWE", Level = OrganizationalUnitLevel.Department, IsActive = true },
                new OrganizationalUnit { CompanyId = company.Id, Name = "Finance", Code = "DEPT-FIN", Level = OrganizationalUnitLevel.Department, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Designations.AnyAsync())
        {
            context.Designations.AddRange(
                new Designation { CompanyId = company.Id, Title = "Software Engineer", Code = "DSG-SWE", IsActive = true },
                new Designation { CompanyId = company.Id, Title = "Senior Software Engineer", Code = "DSG-SR-SWE", IsActive = true },
                new Designation { CompanyId = company.Id, Title = "HR Executive", Code = "DSG-HR-EXEC", IsActive = true },
                new Designation { CompanyId = company.Id, Title = "Accounts Officer", Code = "DSG-ACC-OFF", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Grades.AnyAsync())
        {
            context.Grades.AddRange(
                new Grade { CompanyId = company.Id, Name = "Grade 3", Code = "GR-3", Level = 3, IsActive = true },
                new Grade { CompanyId = company.Id, Name = "Grade 4", Code = "GR-4", Level = 4, IsActive = true },
                new Grade { CompanyId = company.Id, Name = "Grade 5", Code = "GR-5", Level = 5, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.EmploymentTypes.AnyAsync())
        {
            context.EmploymentTypes.AddRange(
                new EmploymentType { CompanyId = company.Id, Name = "Permanent", Code = "ET-PERM", IsActive = true },
                new EmploymentType { CompanyId = company.Id, Name = "Contractual", Code = "ET-CONT", IsActive = true },
                new EmploymentType { CompanyId = company.Id, Name = "Daily Worker", Code = "ET-DAILY", IsActive = true },
                new EmploymentType { CompanyId = company.Id, Name = "Intern", Code = "ET-INTERN", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.EmployeeCategories.AnyAsync())
        {
            context.EmployeeCategories.AddRange(
                new EmployeeCategory { CompanyId = company.Id, Name = "Office Employee", Code = "CAT-OFFICE", IsActive = true },
                new EmployeeCategory { CompanyId = company.Id, Name = "Production Worker", Code = "CAT-WORKER", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Shifts.AnyAsync())
        {
            var officeDays = WorkingDays.Sunday | WorkingDays.Monday | WorkingDays.Tuesday | WorkingDays.Wednesday | WorkingDays.Thursday;
            context.Shifts.AddRange(
                new Shift
                {
                    CompanyId = company.Id,
                    Name = "General Office Shift",
                    Code = "SHIFT-GEN",
                    Type = ShiftType.Fixed,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 30),
                    BreakMinutes = 30,
                    RequiredHours = 8,
                    HalfDayHours = 4,
                    OvertimeAfterHours = 8.5m,
                    LateGraceMinutes = 15,
                    EarlyExitGraceMinutes = 10,
                    CheckInWindowMinutes = 120,
                    CheckOutWindowMinutes = 240,
                    WorkingDays = officeDays,
                    CalculateOvertime = true,
                    HonorHolidayProtection = true,
                    IsActive = true
                },
                new Shift
                {
                    CompanyId = company.Id,
                    Name = "Flexible Software Shift",
                    Code = "SHIFT-FLEX",
                    Type = ShiftType.Flexible,
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(20, 0),
                    BreakMinutes = 30,
                    RequiredHours = 8,
                    HalfDayHours = 4,
                    OvertimeAfterHours = 8.5m,
                    LateGraceMinutes = 0,
                    EarlyExitGraceMinutes = 0,
                    CheckInWindowMinutes = 240,
                    CheckOutWindowMinutes = 240,
                    WorkingDays = officeDays,
                    CalculateOvertime = true,
                    HonorHolidayProtection = true,
                    IsActive = true
                }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.LeaveTypes.AnyAsync())
        {
            context.LeaveTypes.AddRange(
                new LeaveType { CompanyId = company.Id, Name = "Annual Leave", Code = "LV-ANNUAL", AnnualEntitlementDays = 20, MaxConsecutiveDays = 10, AdvanceNoticeDays = 2, ExcludeWeekends = true, ExcludeHolidays = true, IsActive = true },
                new LeaveType { CompanyId = company.Id, Name = "Sick Leave", Code = "LV-SICK", AnnualEntitlementDays = 10, MaxConsecutiveDays = 5, AdvanceNoticeDays = 0, RequiresAttachment = true, ExcludeWeekends = true, ExcludeHolidays = true, IsActive = true },
                new LeaveType { CompanyId = company.Id, Name = "Unpaid Leave", Code = "LV-UNPAID", AnnualEntitlementDays = 0, MaxConsecutiveDays = 30, AdvanceNoticeDays = 5, ExcludeWeekends = false, ExcludeHolidays = false, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Holidays.AnyAsync())
        {
            var thisYear = DateTime.UtcNow.Year;
            context.Holidays.AddRange(
                new Holiday { CompanyId = company.Id, Name = "New Year's Day", Date = new DateOnly(thisYear, 1, 1), Type = HolidayType.PaidPublic, IsActive = true },
                new Holiday { CompanyId = company.Id, Name = "Independence Day", Date = new DateOnly(thisYear, 3, 26), Type = HolidayType.PaidPublic, IsActive = true },
                new Holiday { CompanyId = company.Id, Name = "Company Foundation Day", Date = new DateOnly(thisYear, 9, 10), Type = HolidayType.Company, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.SalaryComponents.AnyAsync())
        {
            context.SalaryComponents.AddRange(
                new SalaryComponent { CompanyId = company.Id, Name = "House Allowance", Code = "EARN-HOUSE", Type = SalaryComponentType.Earning, Method = SalaryCalculationMethod.PercentageOfBasic, Value = 40, IsTaxable = true, IsActive = true },
                new SalaryComponent { CompanyId = company.Id, Name = "Medical Allowance", Code = "EARN-MEDICAL", Type = SalaryComponentType.Earning, Method = SalaryCalculationMethod.FixedAmount, Value = 120, IsTaxable = false, IsActive = true },
                new SalaryComponent { CompanyId = company.Id, Name = "Transport Allowance", Code = "EARN-TRANSPORT", Type = SalaryComponentType.Earning, Method = SalaryCalculationMethod.FixedAmount, Value = 50, IsTaxable = false, IsActive = true },
                new SalaryComponent { CompanyId = company.Id, Name = "Loan Recovery", Code = "DED-LOAN", Type = SalaryComponentType.Deduction, Method = SalaryCalculationMethod.FixedAmount, Value = 0, IsTaxable = false, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.SalaryStructures.AnyAsync())
        {
            var house = await context.SalaryComponents.FirstAsync(c => c.CompanyId == company.Id && c.Code == "EARN-HOUSE");
            var medical = await context.SalaryComponents.FirstAsync(c => c.CompanyId == company.Id && c.Code == "EARN-MEDICAL");
            var transport = await context.SalaryComponents.FirstAsync(c => c.CompanyId == company.Id && c.Code == "EARN-TRANSPORT");

            var structure = new SalaryStructure
            {
                CompanyId = company.Id,
                Name = "Permanent Grade 5",
                Code = "STR-PG5",
                PayrollFrequency = "Monthly",
                ProrationMethod = ProrationMethod.WorkingDays,
                EffectiveDate = new DateOnly(DateTime.UtcNow.Year, 1, 1),
                IsActive = true
            };
            context.SalaryStructures.Add(structure);
            await context.SaveChangesAsync();

            context.SalaryStructureComponents.AddRange(
                new SalaryStructureComponent { SalaryStructureId = structure.Id, SalaryComponentId = house.Id },
                new SalaryStructureComponent { SalaryStructureId = structure.Id, SalaryComponentId = medical.Id },
                new SalaryStructureComponent { SalaryStructureId = structure.Id, SalaryComponentId = transport.Id }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.OvertimePolicies.AnyAsync())
        {
            context.OvertimePolicies.Add(new OvertimePolicy
            {
                CompanyId = company.Id,
                Name = "Standard Overtime Policy",
                MinOvertimeMinutes = 30,
                MaxDailyHours = 4,
                MaxMonthlyHours = 60,
                NormalMultiplier = 1.5m,
                WeekendMultiplier = 2.0m,
                HolidayMultiplier = 2.0m,
                RequiresApproval = true,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }
    }
}
