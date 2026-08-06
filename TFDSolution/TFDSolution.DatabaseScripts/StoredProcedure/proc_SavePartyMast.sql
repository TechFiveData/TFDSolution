CREATE PROCEDURE proc_SavePartyMast
(
    @PartyId UNIQUEIDENTIFIER = NULL,
    @PartyCode NVARCHAR(50) = NULL,
    @PartyType NVARCHAR(50),
    @PartyName NVARCHAR(100),
    @AlternateName NVARCHAR(100),
    @PartyGroup NVARCHAR(50),
    @Currency NVARCHAR(10),
    @Telephone1 NVARCHAR(20),
    @Telephone2 NVARCHAR(20),
    @Mobile NVARCHAR(20),
    @Email NVARCHAR(100),
    @WebSite NVARCHAR(100),
    @Fax NVARCHAR(20),
    @ContactPerson NVARCHAR(100),
    @Industry NVARCHAR(50),
    @PartyStatus SMALLINT,
    @SalesPerson NVARCHAR(50),
    @Remarks NVARCHAR(MAX),

    @BillToName NVARCHAR(100),
    @BillToAddress1 NVARCHAR(200),
    @BillToAddress2 NVARCHAR(200),
    @BillToCity NVARCHAR(50),
    @BillToState NVARCHAR(50),
    @BillToCountry NVARCHAR(50),
    @BillToPincode NVARCHAR(20),
    @BillToContactNo NVARCHAR(20),
    @BillToContactPerson NVARCHAR(100),
    @BillToEmail NVARCHAR(100),

    @ShipToName NVARCHAR(100),
    @ShipToAddress1 NVARCHAR(200),
    @ShipToAddress2 NVARCHAR(200),
    @ShipToCity NVARCHAR(50),
    @ShipToState NVARCHAR(50),
    @ShipToCountry NVARCHAR(50),
    @ShipToPincode NVARCHAR(20),
    @ShipToContactNo NVARCHAR(20),
    @ShipToContactPerson NVARCHAR(100),
    @ShipToEmail NVARCHAR(100),

    @GSTNo NVARCHAR(20),
    @GSTType NVARCHAR(20),
    @PANNo NVARCHAR(20),

    @UserId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @GeneratedPartyCode NVARCHAR(50);
    DECLARE @Action NVARCHAR(10), @Response NVARCHAR(200), @IsSuccess BIT;
	-- Declare a table variable to hold the result from m_getNextCode
	DECLARE @TempTable TABLE (LastCode NVARCHAR(50), Prefix NVARCHAR(5));
    BEGIN TRY
        IF ISNULL(@PartyId, '00000000-0000-0000-0000-000000000000')= '00000000-0000-0000-0000-000000000000'
        BEGIN
            SET @PartyId = NEWID();
            -- Call helper SP to get PartyCode
            INSERT INTO @TempTable
			EXEC m_getNextCode 'm_PartyMast';
			SELECT @GeneratedPartyCode = LastCode FROM @TempTable;
			-- Assign to @PartyCode if not already set
            SET @PartyCode = ISNULL(@PartyCode, @GeneratedPartyCode);
        END

        IF EXISTS (SELECT 1 FROM m_PartyMast WHERE PartyId = @PartyId)
        BEGIN
            -- Update
            UPDATE m_PartyMast
            SET
                PartyCode = @PartyCode,
                PartyType = @PartyType,
                PartyName = @PartyName,
                AlternateName = @AlternateName,
                PartyGroup = @PartyGroup,
                Currency = @Currency,
                Telephone1 = @Telephone1,
                Telephone2 = @Telephone2,
                Mobile = @Mobile,
                Email = @Email,
                WebSite = @WebSite,
                Fax = @Fax,
                ContactPerson = @ContactPerson,
                Industry = @Industry,
                PartyStatus = @PartyStatus,
                SalesPerson = @SalesPerson,
                Remarks = @Remarks,

                BillToName = @BillToName,
                BillToAddress1 = @BillToAddress1,
                BillToAddress2 = @BillToAddress2,
                BillToCity = @BillToCity,
                BillToState = @BillToState,
                BillToCountry = @BillToCountry,
                BillToPincode = @BillToPincode,
                BillToContactNo = @BillToContactNo,
                BillToContactPerson = @BillToContactPerson,
                BillToEmail = @BillToEmail,

                ShipToName = @ShipToName,
                ShipToAddress1 = @ShipToAddress1,
                ShipToAddress2 = @ShipToAddress2,
                ShipToCity = @ShipToCity,
                ShipToState = @ShipToState,
                ShipToCountry = @ShipToCountry,
                ShipToPincode = @ShipToPincode,
                ShipToContactNo = @ShipToContactNo,
                ShipToContactPerson = @ShipToContactPerson,
                ShipToEmail = @ShipToEmail,

                GSTNo = @GSTNo,
                GSTType = @GSTType,
                PANNo = @PANNo,
                UpdatedBy = @UserId,
                UpdatedOn = GETDATE()
            WHERE PartyId = @PartyId;
        END
        ELSE
        BEGIN
            -- Insert
            INSERT INTO m_PartyMast (
                PartyId, PartyCode, PartyType, PartyName, AlternateName, PartyGroup, Currency, Telephone1, Telephone2,
                Mobile, Email, WebSite, Fax, ContactPerson, Industry, PartyStatus, SalesPerson, Remarks,

                BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, BillToCountry, BillToPincode,
                BillToContactNo, BillToContactPerson, BillToEmail,

                ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, ShipToCountry, ShipToPincode,
                ShipToContactNo, ShipToContactPerson, ShipToEmail,

                GSTNo, GSTType, PANNo, CreatedBy, CreatedOn
            )
            VALUES (
                @PartyId, @PartyCode, @PartyType, @PartyName, @AlternateName, @PartyGroup, @Currency, @Telephone1, @Telephone2,
                @Mobile, @Email, @WebSite, @Fax, @ContactPerson, @Industry, @PartyStatus, @SalesPerson, @Remarks,

                @BillToName, @BillToAddress1, @BillToAddress2, @BillToCity, @BillToState, @BillToCountry, @BillToPincode,
                @BillToContactNo, @BillToContactPerson, @BillToEmail,

                @ShipToName, @ShipToAddress1, @ShipToAddress2, @ShipToCity, @ShipToState, @ShipToCountry, @ShipToPincode,
                @ShipToContactNo, @ShipToContactPerson, @ShipToEmail,

                @GSTNo, @GSTType, @PANNo, @UserId, GETDATE()
            )
        END

        SET @Action = 'Save';
        SET @Response = 'Record saved successfully.';
        SET @IsSuccess = 1;
    END TRY
    BEGIN CATCH
        SET @Action = 'Error';
        SET @Response = ERROR_MESSAGE();
        SET @IsSuccess = 0;
    END CATCH

    SELECT @Action AS Action, @PartyId AS PrimaryId, @IsSuccess AS IsSuccess, @Response AS Response
END
