SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @i int = 1;
WHILE @i <= 10
BEGIN
    DECLARE @old nvarchar(20) = N'TBM' + RIGHT(N'000' + CONVERT(nvarchar(10), @i), 3);
    DECLARE @new nvarchar(20) = N'TB'  + RIGHT(N'000' + CONVERT(nvarchar(10), @i), 3);

    IF EXISTS (SELECT 1 FROM dbo.Devices WHERE Code = @old)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Devices WHERE Code = @new)
        BEGIN
            UPDATE dbo.Devices SET Code = @new WHERE Code = @old;
        END
        ELSE
        BEGIN
            DECLARE @oldId int = (SELECT TOP (1) Id FROM dbo.Devices WHERE Code = @old);
            DECLARE @newId int = (SELECT TOP (1) Id FROM dbo.Devices WHERE Code = @new);
            UPDATE dbo.DeviceAssignments SET DeviceId = @newId WHERE DeviceId = @oldId;
            DELETE FROM dbo.Devices WHERE Id = @oldId;
        END
    END

    DECLARE @legacyNote nvarchar(255) = N'Dữ liệu mẫu cấp phát #' + RIGHT(N'00' + CONVERT(nvarchar(10), @i), 2);
    DECLARE @newNote nvarchar(255) = CASE @i
        WHEN 1 THEN N'Bàn giao máy tính để bàn phục vụ công việc tại Phòng CNTT.'
        WHEN 2 THEN N'Bàn giao laptop phục vụ công việc và họp trực tuyến.'
        WHEN 3 THEN N'Bàn giao máy in dùng chung cho Phòng Kế toán.'
        WHEN 4 THEN N'Bàn giao màn hình bổ sung cho vị trí làm việc.'
        WHEN 5 THEN N'Bàn giao switch phục vụ hệ thống mạng nội bộ.'
        WHEN 6 THEN N'Bàn giao máy chủ phục vụ hệ thống nội bộ; đã thu hồi để bảo trì.'
        WHEN 7 THEN N'Bàn giao UPS bảo vệ nguồn cho thiết bị hạ tầng; đã thu hồi sau sự cố.'
        WHEN 8 THEN N'Bàn giao camera giám sát khu vực hành chính; đã thu hồi để thanh lý.'
        WHEN 9 THEN N'Bàn giao máy chấm công cho Phòng Nhân sự; đã hoàn tất thu hồi.'
        WHEN 10 THEN N'Bàn giao máy quét mã vạch cho Phòng Kho vận; đã hoàn tất thu hồi.'
    END;
    UPDATE dbo.DeviceAssignments SET Note = @newNote WHERE Note = @legacyNote;

    SET @i += 1;
END


COMMIT TRANSACTION;
