DROP PROCEDURE IF EXISTS [dbo].[proc_GetSPParameterList]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 18/03/2025
-- Description:	<Description,,>
-- =============================================
/*
EXEC proc_GetSPParameterList @SPName = 'proc_GetSPParameterList'
 */
CREATE PROCEDURE [dbo].[proc_GetSPParameterList]
    @SPName NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
	
    SELECT  
        p.PARAMETER_NAME AS ParameterName,
        p.DATA_TYPE AS DataType,
        p.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
        p.PARAMETER_MODE AS ParameterMode
    FROM INFORMATION_SCHEMA.PARAMETERS p
    WHERE p.SPECIFIC_NAME = @SPName
    ORDER BY p.ORDINAL_POSITION;
END
GO
