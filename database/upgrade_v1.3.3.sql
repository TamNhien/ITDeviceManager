SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @i int = 1;
WHILE @i <= 10
BEGIN
    DECLARE @old nvarchar(20) = N'NVM' + RIGHT(N'000' + CONVERT(nvarchar(10), @i), 3);
    DECLARE @new nvarchar(20) = N'NV'  + RIGHT(N'000' + CONVERT(nvarchar(10), @i), 3);

    IF EXISTS (SELECT 1 FROM dbo.Employees WHERE Code = @old)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE Code = @new)
        BEGIN
            UPDATE dbo.Employees SET Code = @new WHERE Code = @old;
        END
        ELSE
        BEGIN
            DECLARE @oldId int = (SELECT TOP (1) Id FROM dbo.Employees WHERE Code = @old);
            DECLARE @newId int = (SELECT TOP (1) Id FROM dbo.Employees WHERE Code = @new);
            UPDATE dbo.DeviceAssignments SET EmployeeId = @newId WHERE EmployeeId = @oldId;
            DELETE FROM dbo.Employees WHERE Id = @oldId;
        END
    END

    SET @i += 1;
END

COMMIT TRANSACTION;
