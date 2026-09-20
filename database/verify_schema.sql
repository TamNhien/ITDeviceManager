SELECT
    COL_LENGTH(N'dbo.Users', N'Email') AS UsersEmailColumn,
    COL_LENGTH(N'dbo.Users', N'PhoneNumber') AS UsersPhoneNumberColumn,
    COL_LENGTH(N'dbo.Users', N'PasswordSalt') AS PasswordSaltShouldBeNull,
    OBJECT_ID(N'dbo.PasswordResetTokens', N'U') AS PasswordResetTokensObjectId;

SELECT name
FROM sys.indexes
WHERE object_id = OBJECT_ID(N'dbo.Users')
  AND name IN (N'IX_Users_Email', N'IX_Users_PhoneNumber');

SELECT
    OBJECT_NAME(c.object_id) AS TableName,
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS SqlType
FROM sys.columns c
WHERE (c.object_id = OBJECT_ID(N'dbo.Users') AND c.name IN (N'FullName', N'PhoneNumber'))
   OR (c.object_id = OBJECT_ID(N'dbo.Employees') AND c.name IN (N'FullName', N'Email', N'Phone'))
   OR (c.object_id = OBJECT_ID(N'dbo.Departments') AND c.name = N'Name')
   OR (c.object_id = OBJECT_ID(N'dbo.DeviceTypes') AND c.name = N'Name')
   OR (c.object_id = OBJECT_ID(N'dbo.Devices') AND c.name = N'Name')
   OR (c.object_id = OBJECT_ID(N'dbo.DeviceAssignments') AND c.name = N'Note')
ORDER BY TableName, ColumnName;

SELECT N'Roles' AS TableName, COUNT(*) AS RowCount FROM dbo.Roles
UNION ALL SELECT N'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT N'Departments', COUNT(*) FROM dbo.Departments
UNION ALL SELECT N'Employees', COUNT(*) FROM dbo.Employees
UNION ALL SELECT N'DeviceTypes', COUNT(*) FROM dbo.DeviceTypes
UNION ALL SELECT N'Devices', COUNT(*) FROM dbo.Devices
UNION ALL SELECT N'DeviceAssignments', COUNT(*) FROM dbo.DeviceAssignments
UNION ALL SELECT N'PasswordResetTokens', COUNT(*) FROM dbo.PasswordResetTokens;

SELECT Code, Name, Status
FROM dbo.Devices
WHERE Code LIKE N'TB%'
ORDER BY Code;

SELECT
    SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS Available,
    SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) AS InUse,
    SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS Repair,
    SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) AS Broken,
    SUM(CASE WHEN Status = 5 THEN 1 ELSE 0 END) AS Retired
FROM dbo.Devices;

SELECT TOP (10) d.Code, a.Note
FROM dbo.DeviceAssignments a
JOIN dbo.Devices d ON d.Id = a.DeviceId
ORDER BY a.AssignedDate DESC;

-- V1.3.5: this result set must return 0 rows.
SELECT Id, Code, Name, SerialNumber
FROM dbo.Devices
WHERE SerialNumber LIKE N'DEMO-SN-%';

-- V1.4.3: maintenance / repair / warranty module.
SELECT OBJECT_ID(N'dbo.DeviceMaintenances', N'U') AS DeviceMaintenancesObjectId;

IF OBJECT_ID(N'dbo.DeviceMaintenances', N'U') IS NOT NULL
BEGIN
    SELECT COUNT(*) AS DeviceMaintenanceRowCount
    FROM dbo.DeviceMaintenances;

    SELECT TOP (10)
        m.Code,
        d.Code AS DeviceCode,
        d.Name AS DeviceName,
        m.Type,
        m.ReceivedDate,
        m.CompletedDate,
        m.Status,
        m.Cost
    FROM dbo.DeviceMaintenances m
    JOIN dbo.Devices d ON d.Id = m.DeviceId
    ORDER BY m.ReceivedDate DESC, m.Id DESC;
END;
