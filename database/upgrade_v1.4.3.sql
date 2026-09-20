SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.DeviceMaintenances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeviceMaintenances
    (
        Id int IDENTITY(1,1) NOT NULL,
        Code nvarchar(20) NOT NULL,
        DeviceId int NOT NULL,
        Type int NOT NULL,
        ReceivedDate date NOT NULL,
        CompletedDate date NULL,
        Provider nvarchar(200) NULL,
        Cost decimal(18,2) NULL,
        IssueDescription nvarchar(1000) NOT NULL,
        Resolution nvarchar(1000) NULL,
        Status int NOT NULL,
        PreviousDeviceStatus int NOT NULL,
        ResultDeviceStatus int NULL,
        Note nvarchar(1000) NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_DeviceMaintenances_CreatedAt DEFAULT SYSdatetime(),
        UpdatedAt datetime2 NOT NULL CONSTRAINT DF_DeviceMaintenances_UpdatedAt DEFAULT SYSdatetime(),
        CONSTRAINT PK_DeviceMaintenances PRIMARY KEY (Id),
        CONSTRAINT FK_DeviceMaintenances_Devices_DeviceId
            FOREIGN KEY (DeviceId) REFERENCES dbo.Devices(Id) ON DELETE NO ACTION,
        CONSTRAINT CK_DeviceMaintenances_Type CHECK (Type BETWEEN 1 AND 5),
        CONSTRAINT CK_DeviceMaintenances_Status CHECK (Status BETWEEN 1 AND 4),
        CONSTRAINT CK_DeviceMaintenances_PreviousDeviceStatus CHECK (PreviousDeviceStatus BETWEEN 1 AND 5),
        CONSTRAINT CK_DeviceMaintenances_ResultDeviceStatus CHECK (ResultDeviceStatus IS NULL OR ResultDeviceStatus BETWEEN 1 AND 5)
    );

    CREATE UNIQUE INDEX IX_DeviceMaintenances_Code
        ON dbo.DeviceMaintenances(Code);

    CREATE INDEX IX_DeviceMaintenances_DeviceId_ReceivedDate
        ON dbo.DeviceMaintenances(DeviceId, ReceivedDate DESC);

    CREATE INDEX IX_DeviceMaintenances_Status
        ON dbo.DeviceMaintenances(Status);
END;

COMMIT TRANSACTION;
