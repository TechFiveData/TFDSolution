DROP PROCEDURE IF EXISTS [dbo].[proc_AddCompany]
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Amit Gorvadiya
-- Create date: 03/02/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE proc_AddCompany
	-- Add the parameters for the stored procedure here
	@CompanyCode as varchar(30),
	@CompanyName as varchar(50),
	@CreatedBy as varchar(30),
	@Email as varchar(15) = NULL,
	@Phone as varchar(15) = NULL,
	@Mobile as varchar(15) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @CompId AS UniqueIdentifier
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0
	 IF @CompanyCode IS NULL OR @CompanyName IS NULL OR @Phone IS NULL
	BEGIN
         SET @MessageText = 'Missing required fields for adding new company.'
    END

    -- Insert statements for procedure here
	IF EXISTS (SELECT TOP 1 * FROM m_CompanyMast WITH(NOLOCK) WHERE CompanyName = @CompanyName)
	BEGIN
		SET @MessageText = 'Company Name is already exists.'
	END
	ELSE
	BEGIN
		SET @CompId = NEWID()

		DECLARE @userid AS UniqueIdentifier = CAST(CAST('850f1eb9-d6ad-483e-941c-29efc4fcded2' as char(36)) as uniqueidentifier)
		
       INSERT INTO m_CompanyMast (CompanyId, CompanyCode, CompanyName, Phone, Mobile, Email, CreatedBy,CreatedOn)
       VALUES (@CompId,@CompanyCode, @CompanyName, @Phone, @Mobile, @Email, @userid,GETDATE())
		set @IsSuccess = 1
       SET @MessageText = 'Company has been added successfully'

	END

	SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Add' as Action, @CompId as PrimaryId

END
