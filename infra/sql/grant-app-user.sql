SET NOCOUNT ON;

IF '$(UserName)' LIKE '%[^A-Za-z0-9-]%' OR LEN('$(UserName)') = 0
BEGIN
    THROW 50001, 'UserName must be the managed identity name: letters, digits and hyphens only.', 1;
END

IF TRY_CAST('$(UserClientId)' AS uniqueidentifier) IS NULL
BEGIN
    THROW 50002, 'UserClientId must be the client id (a GUID) of the managed identity.', 1;
END

IF DATABASE_PRINCIPAL_ID('claims_app') IS NULL
BEGIN
    THROW 50003, 'The role claims_app does not exist: run the migrations before this script.', 1;
END

DECLARE @sid varbinary(16) = CAST(CAST('$(UserClientId)' AS uniqueidentifier) AS varbinary(16));

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '$(UserName)' AND sid <> @sid)
BEGIN
    DECLARE @dropUser nvarchar(max) = N'DROP USER ' + QUOTENAME('$(UserName)') + N';';

    EXEC (@dropUser);
END

IF DATABASE_PRINCIPAL_ID('$(UserName)') IS NULL
BEGIN
    DECLARE @createUser nvarchar(max) =
        N'CREATE USER ' + QUOTENAME('$(UserName)') + N' WITH SID = ' + CONVERT(nvarchar(34), @sid, 1) + N', TYPE = E;';

    EXEC (@createUser);
END

DECLARE @roles TABLE (RoleName sysname NOT NULL);
INSERT INTO @roles (RoleName) VALUES ('db_datareader'), ('db_datawriter'), ('db_ddladmin'), ('claims_app');

DECLARE @addMember nvarchar(max) = N'';

SELECT @addMember = @addMember + N'ALTER ROLE ' + QUOTENAME(RoleName) + N' ADD MEMBER ' + QUOTENAME('$(UserName)') + N';'
FROM @roles
WHERE ISNULL(IS_ROLEMEMBER(RoleName, '$(UserName)'), 0) = 0;

IF LEN(@addMember) > 0
BEGIN
    EXEC (@addMember);
END
