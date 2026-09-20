using Microsoft.EntityFrameworkCore;

namespace ITDeviceManager.Data;

/// <summary>
/// V1.3.5 data cleanup: replaces only legacy DEMO-SN-* serials created by
/// early sample-data versions. Real serial numbers entered by users are never overwritten.
/// </summary>
public static class SchemaUpgradeV135
{
    private static readonly IReadOnlyDictionary<string, string> SerialByCode =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TB001"] = "DL7020-260001",
            ["TB002"] = "DL5450-260002",
            ["TB003"] = "HP4003-260003",
            ["TB004"] = "DP2425-260004",
            ["TB005"] = "CBS350-260005",
            ["TB006"] = "HPEML30-260006",
            ["TB007"] = "APC1500-260007",
            ["TB008"] = "HK4MP-260008",
            ["TB009"] = "RJX628-260009",
            ["TB010"] = "ZDS2208-260010",
        };

    public static async Task UpgradeAsync(AppDbContext db)
    {
        var legacyDevices = await db.Devices
            .Where(x => x.SerialNumber != null && x.SerialNumber.StartsWith("DEMO-SN-"))
            .ToListAsync();

        if (legacyDevices.Count == 0)
            return;

        foreach (var device in legacyDevices)
        {
            // V1.3.4 already renames TBMxxx -> TBxxx, but support both forms
            // so this cleanup is safe even if a database skipped an earlier migration.
            var code = device.Code.StartsWith("TBM", StringComparison.OrdinalIgnoreCase)
                ? "TB" + device.Code[3..]
                : device.Code;

            if (!SerialByCode.TryGetValue(code, out var serial))
                continue;

            // Do not create a duplicate serial if the target value already belongs
            // to a different device.
            var duplicate = await db.Devices.AnyAsync(x =>
                x.Id != device.Id && x.SerialNumber == serial);
            if (duplicate)
                continue;

            device.SerialNumber = serial;
        }

        await db.SaveChangesAsync();
    }
}
