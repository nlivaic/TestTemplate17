-- Executed only on Azure SQL
IF SERVERPROPERTY('EngineEdition') > 4
BEGIN
	CREATE USER [$SqlUsersGroupNameVariable$] FROM EXTERNAL PROVIDER;
    EXEC sp_addrolemember 'db_datareader', '$SqlUsersGroupNameVariable$'
    EXEC sp_addrolemember 'db_datawriter', '$SqlUsersGroupNameVariable$'
END
