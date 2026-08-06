DROP PROCEDURE IF EXISTS proc_SaveUserCompanySetting
GO

CREATE PROCEDURE proc_SaveUserCompanySetting
    @CompanyTable dbo.UDT_UserCompanySetting READONLY
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
	DECLARE @userid VARCHAR(50)
	SET @userid =(SELECT DISTINCT UserId FROM @CompanyTable)
	IF (LEN(@userid)>0 AND ISNULL(@userid,'00000000-0000-0000-0000-000000000000')!='00000000-0000-0000-0000-000000000000')
	BEGIN
        -- Update financial year if the user and company combination exists
        UPDATE uc
        SET uc.DefaultFiancialYearId = ISNULL(ct.FinancialYearId,uc.DefaultFiancialYearId)
        FROM m_UserSettings uc
        INNER JOIN @CompanyTable ct
            ON uc.UserId = @userid AND uc.CompanyId = ct.CompanyId;

        -- Delete only those companies that are not in the input table for the given user
        DELETE FROM m_UserSettings
        WHERE UserId =@userid
          AND CompanyId NOT IN (SELECT CompanyId FROM @CompanyTable);

        -- Insert new user-company combinations that do not already exist
        INSERT INTO m_UserSettings (UserId, CompanyId, DefaultFiancialYearId)
        SELECT 
            ct.UserId, 
            ct.CompanyId, 
            ISNULL(ct.FinancialYearId, cf.FiancialYearId) -- Default financial year
        FROM @CompanyTable ct
		 LEFT JOIN m_UserSettings uc ON uc.UserId = ct.UserId AND uc.CompanyId = ct.CompanyId
        LEFT JOIN m_CompanyFinancials cf ON cf.CompanyId = ct.CompanyId AND YEAR(cf.EndDate) = YEAR(getdate())
        WHERE uc.UserId IS NULL AND uc.CompanyId IS NULL;

        COMMIT TRANSACTION;
	END
    END TRY
    BEGIN CATCH
        -- Rollback the transaction in case of an error
        ROLLBACK TRANSACTION;

        -- Raise an error with details of the exception
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
