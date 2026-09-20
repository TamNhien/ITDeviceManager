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
