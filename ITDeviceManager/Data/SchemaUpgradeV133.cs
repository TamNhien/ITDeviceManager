using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

public static class SchemaUpgradeV133
{
    public static async Task UpgradeAsync(AppDbContext db)
    {
        // V1.3.3 standardizes only the ten seeded employee codes.
        // DeviceAssignments use EmployeeId foreign keys, so changing Code preserves history.
        for (var i = 1; i <= 10; i++)
        {
            var oldCode = $"NVM{i:D3}";
            var newCode = $"NV{i:D3}";

            var oldEmployee = await db.Employees.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Code == oldCode);
            if (oldEmployee is null)
                continue;

            var newEmployee = await db.Employees.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Code == newCode);
            if (newEmployee is null)
            {
                oldEmployee.Code = newCode;
                await db.SaveChangesAsync();
                continue;
            }

            if (newEmployee.Id == oldEmployee.Id)
                continue;

            // If both codes exist because a previous seed attempt inserted both rows,
            // keep the NV row, move assignment history to it, then remove the old sample row.
            await db.DeviceAssignments
                .Where(x => x.EmployeeId == oldEmployee.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EmployeeId, newEmployee.Id));

            db.Employees.Remove(oldEmployee);
            await db.SaveChangesAsync();
        }
    }
}
