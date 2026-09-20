SELECT
    COL_LENGTH(N'dbo.Users', N'Email') AS UsersEmailColumn,
    COL_LENGTH(N'dbo.Users', N'PhoneNumber') AS UsersPhoneNumberColumn,
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
