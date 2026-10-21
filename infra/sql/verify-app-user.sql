SET NOCOUNT ON;

IF DATABASE_PRINCIPAL_ID('$(AppName)') IS NULL
BEGIN
    THROW 50010, 'The database user of the web app does not exist.', 1;
END

DECLARE @canRead int = 0;
DECLARE @canInsert int = 0;
DECLARE @canUpdate int = 1;
DECLARE @canDelete int = 1;

EXECUTE AS USER = '$(AppName)';

SELECT
    @canRead = HAS_PERMS_BY_NAME('dbo.ClaimAuditLog', 'OBJECT', 'SELECT'),
    @canInsert = HAS_PERMS_BY_NAME('dbo.ClaimAuditLog', 'OBJECT', 'INSERT'),
    @canUpdate = HAS_PERMS_BY_NAME('dbo.ClaimAuditLog', 'OBJECT', 'UPDATE'),
    @canDelete = HAS_PERMS_BY_NAME('dbo.ClaimAuditLog', 'OBJECT', 'DELETE');

REVERT;

IF @canRead <> 1 OR @canInsert <> 1
BEGIN
    THROW 50011, 'The web app user must be able to read and append audit rows.', 1;
END

IF @canUpdate <> 0 OR @canDelete <> 0
BEGIN
    THROW 50012, 'The web app user must not be able to update or delete audit rows.', 1;
END

SELECT 'audit permissions as designed' AS Result;
