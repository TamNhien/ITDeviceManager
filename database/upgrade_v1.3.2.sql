/* IT Device Manager V1.3.2 - Phone number local-format migration */
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Users', N'PhoneNumber') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT CanonicalPhone
        FROM (
            SELECT
                CASE
                    WHEN PhoneNumber LIKE N'+84%' THEN N'0' + SUBSTRING(PhoneNumber, 4, 64)
                    WHEN PhoneNumber LIKE N'84%' AND PhoneNumber NOT LIKE N'0%' AND LEN(PhoneNumber) >= 10
                        THEN N'0' + SUBSTRING(PhoneNumber, 3, 64)
                    ELSE PhoneNumber
                END AS CanonicalPhone
            FROM dbo.Users
            WHERE PhoneNumber IS NOT NULL AND PhoneNumber <> N''
        ) AS CanonicalPhones
        GROUP BY CanonicalPhone
        HAVING COUNT(*) > 1)
    BEGIN
        THROW 51032, N'Có số điện thoại trùng sau khi đổi +84 sang 0. Hãy xử lý bản ghi trùng trước.', 1;
    END;

    UPDATE dbo.Users
    SET PhoneNumber = N'0' + SUBSTRING(PhoneNumber, 4, 64)
    WHERE PhoneNumber LIKE N'+84%';

    UPDATE dbo.Users
    SET PhoneNumber = N'0' + SUBSTRING(PhoneNumber, 3, 64)
    WHERE PhoneNumber LIKE N'84%'
      AND PhoneNumber NOT LIKE N'0%'
      AND LEN(PhoneNumber) >= 10;
END;

IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Employees', N'Phone') IS NOT NULL
BEGIN
    UPDATE dbo.Employees
    SET Phone = N'0' + SUBSTRING(Phone, 4, 64)
    WHERE Phone LIKE N'+84%';

    UPDATE dbo.Employees
    SET Phone = N'0' + SUBSTRING(Phone, 3, 64)
    WHERE Phone LIKE N'84%'
      AND Phone NOT LIKE N'0%'
      AND LEN(Phone) >= 10;
END;
