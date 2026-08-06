CREATE OR ALTER PROCEDURE [dbo].[proc_SaveUserDetail]
    @UserId UNIQUEIDENTIFIER = NULL,
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @EmailId NVARCHAR(50),
    @UserName NVARCHAR(50),
    @IsActive BIT,
    @Address1 NVARCHAR(200) = NULL,
    @Address2 NVARCHAR(200) = NULL,
    @City INT = NULL,
    @StateId INT = NULL,
    @CountryId INT = NULL,
    @ZipCode NVARCHAR(6) = NULL,
    @Pincode NVARCHAR(6) = NULL,
    @ContactNo NVARCHAR(10) = NULL,
    @ContactPerson NVARCHAR(50) = NULL,
    @ContactEmail NVARCHAR(50) = NULL,
    @MobileNo NVARCHAR(15) = NULL,
    @RoleId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @Action NVARCHAR(20), 
        @Response NVARCHAR(200),
        @IsSuccess BIT = 0,
        @HasError BIT = 0;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- UPDATE operation
        IF @UserId IS NOT NULL AND EXISTS (SELECT 1 FROM m_UserMast WITH(NOLOCK) WHERE UserId = @UserId)
        BEGIN
            IF EXISTS (SELECT 1 FROM m_UserMast  WITH(NOLOCK)  WHERE UserId != @UserId AND EmailId = @EmailId)
            BEGIN
                SET @Action = 'Validation';
                SET @Response = 'EmailId already exists.';
                SET @HasError = 1;
            END
            ELSE
            BEGIN
                -- Update m_UserMast
                UPDATE m_UserMast
                SET 
                    FirstName = @FirstName,
                    LastName = @LastName,
                    EmailId = @EmailId,
                    IsActive = @IsActive,
                    UpdatedOn = GETDATE(),
                    RoleId = @RoleId
                WHERE UserId = @UserId;
            END
        END
        ELSE
        BEGIN
            -- INSERT operation
            IF EXISTS (SELECT 1 FROM m_UserMast WITH(NOLOCK) WHERE EmailId = @EmailId)
            BEGIN
                SET @Action = 'Validation';
                SET @Response = 'EmailId already exists.';
                SET @HasError = 1;
            END
            ELSE IF EXISTS (SELECT 1 FROM m_UserMast WITH(NOLOCK) WHERE UserName = @UserName)
            BEGIN
                SET @Action = 'Validation';
                SET @Response = 'Username already exists.';
                SET @HasError = 1;
            END
            ELSE
            BEGIN
                SET @UserId = NEWID();
                INSERT INTO m_UserMast (UserId, FirstName, LastName, EmailId, UserName, IsActive, CreatedOn, RoleId)
                VALUES (@UserId, @FirstName, @LastName, @EmailId, @UserName, @IsActive, GETDATE(), @RoleId);
            END
        END

        -- If validation failed, rollback and return
        IF @HasError = 1
        BEGIN
            ROLLBACK;
            SELECT @IsSuccess AS IsSuccess, @Response AS Response,@Action AS Action, @UserId AS PrimaryId;
            RETURN;
        END

        -- Insert/Update m_UserDetails
        IF EXISTS (SELECT 1 FROM m_UserDetails  WITH(NOLOCK)  WHERE UserId = @UserId)
        BEGIN
            UPDATE m_UserDetails
            SET 
                Address1 = @Address1,
                Address2 = @Address2,
                City = @City,
                StateId = @StateId,
                CountryId = @CountryId,
                Pincode = @Pincode,
                ContactNo = @ContactNo,
                ContactPerson = @ContactPerson,
                ContactEmail = @ContactEmail,
                MobileNumber = @MobileNo
            WHERE UserId = @UserId;
        END
        ELSE
        BEGIN
            INSERT INTO m_UserDetails (UserId, Address1, Address2, City, StateId, CountryId, Pincode, ContactNo, ContactPerson, ContactEmail, MobileNumber)
            VALUES (@UserId, @Address1, @Address2, @City, @StateId, @CountryId, @Pincode, @ContactNo, @ContactPerson, @ContactEmail, @MobileNo);
        END

        -- Success response
        SET @Action = 'Save';
        SET @Response = 'Record saved successfully.';
        SET @IsSuccess = 1;

        COMMIT;
		SELECT @IsSuccess AS IsSuccess, @Response AS Response,@Action AS Action, @UserId AS PrimaryId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        SET @Action = 'Error';
		SELECT 0 AS IsSuccess, @ErrMsg AS Response,@Action AS Action, @UserId AS PrimaryId;
    END CATCH
END