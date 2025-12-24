USE master
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TestTemplate17Db')
BEGIN
  CREATE DATABASE TestTemplate17Db;
END;
GO

USE TestTemplate17Db;
GO

IF NOT EXISTS (SELECT 1
                 FROM sys.server_principals
                WHERE [name] = N'TestTemplate17Db_Login' 
                  AND [type] IN ('C','E', 'G', 'K', 'S', 'U'))
BEGIN
    CREATE LOGIN TestTemplate17Db_Login
        WITH PASSWORD = '<DB_PASSWORD>';
END;
GO  

IF NOT EXISTS (select * from sys.database_principals where name = 'TestTemplate17Db_User')
BEGIN
    CREATE USER TestTemplate17Db_User FOR LOGIN TestTemplate17Db_Login;
END;
GO  


EXEC sp_addrolemember N'db_datareader', N'TestTemplate17Db_User';
GO

EXEC sp_addrolemember N'db_datawriter', N'TestTemplate17Db_User';
GO

EXEC sp_addrolemember N'db_ddladmin', N'TestTemplate17Db_User';
GO
