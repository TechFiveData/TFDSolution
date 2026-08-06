DROP PROCEDURE IF EXISTS proc_GetUserDetail
GO

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Get User Master and user details if userid supplied else all list>
-- =============================================

CREATE PROCEDURE proc_GetUserDetail
    @UserId VARCHAR(50) = null
AS
BEGIN
    SET NOCOUNT ON;
	BEGIN TRY
	
		SELECT UM.UserId			
		,FirstName,LastName	,EmailId,UserName,IsActive	,CreatedOn	,UpdatedOn		
		,LastLoginDate	,PasswordExpiredOn, ISNULL(Status,0) AS Status, RoleId AS UserRole
		--userdetails
		,Address1,Address2	,City,StateId AS State	,CountryId AS Country 				
		,Pincode,ContactNo,ContactPerson	,ContactEmail, MobileNumber
		FROM m_UserMast UM WITH (NOLOCK)
		LEFT JOIN m_userDetails UD WITH (NOLOCK) ON UM.UserId = UD.UserId		
		WHERE UM.UserId = (CASE WHEN ISNULL(@UserId,'') = '' THEN UM.UserId ELSE @UserId END)

		SELECT  US.UserId, US.CompanyId ,US.DefaultFiancialYearId
		FROM m_UserMast UM WITH (NOLOCK)
		INNER JOIN m_UserSettings US WITH (NOLOCK) ON UM.UserId = US.UserId
		WHERE UM.UserId = (CASE WHEN ISNULL(@UserId,'') = '' THEN UM.UserId ELSE @UserId END)
		

	END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT, @ErrState INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY(), @ErrState = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSeverity, @ErrState);
    END CATCH
END
GO