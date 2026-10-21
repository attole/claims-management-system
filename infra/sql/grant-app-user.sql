SET NOCOUNT ON;

IF '$(AppName)' LIKE '%[^A-Za-z0-9-]%' OR LEN('$(AppName)') = 0
BEGIN
    THROW 50001, 'AppName must be the web app name: letters, digits and hyphens only.', 1;
END

IF TRY_CAST('$(AppClientId)' AS uniqueidentifier) IS NULL
BEGIN
    THROW 50002, 'AppClientId must be the client id (a GUID) of the web app managed identity.', 1;
END

IF DATABASE_PRINCIPAL_ID('claims_app') IS NULL
BEGIN
    THROW 50003, 'The role claims_app does not exist: run the migrations before this script.', 1;
END

IF DATABASE_PRINCIPAL_ID('$(AppName)') IS NULL
BEGIN
    DECLARE @sid varbinary(16) = CAST(CAST('$(AppClientId)' AS uniqueidentifier) AS varbinary(16));
    DECLARE @createUser nvarchar(max) =
        N'CREATE USER ' + QUOTENAME('$(AppName)') + N' WITH SID = ' + CONVERT(nvarchar(34), @sid, 1) + N', TYPE = E;';

    EXEC (@createUser);
END

DECLARE @roles TABLE (RoleName sysname NOT NULL);
INSERT INTO @roles (RoleName) VALUES ('db_datareader'), ('db_datawriter'), ('db_ddladmin'), ('claims_app');

DECLARE @addMember nvarchar(max) = N'';

SELECT @addMember = @addMember + N'ALTER ROLE ' + QUOTENAME(RoleName) + N' ADD MEMBER ' + QUOTENAME('$(AppName)') + N';'
FROM @roles
WHERE ISNULL(IS_ROLEMEMBER(RoleName, '$(AppName)'), 0) = 0;

IF LEN(@addMember) > 0
BEGIN
    EXEC (@addMember);
END
