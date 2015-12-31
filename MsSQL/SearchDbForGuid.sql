/*
http://stackoverflow.com/questions/970477/sql-server-search-all-tables-for-a-particular-guid
    Search all tables in the database for a guid
      6/9/2009: Removed the IF EXISTS to double hit the database
*/

--DECLARE @searchValue uniqueidentifier
--SET @searchValue = '{2A6814B9-8261-452D-A144-13264433864E}'

DECLARE abc CURSOR FOR
    SELECT 
    	c.TABLE_NAME, c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.Columns c
    	INNER JOIN INFORMATION_SCHEMA.Tables t
    	ON c.TABLE_NAME = t.TABLE_NAME
    	AND t.TABLE_TYPE = 'BASE TABLE'
    WHERE DATA_TYPE = 'uniqueidentifier'

DECLARE @tableSchema varchar(200)
DECLARE @tableName varchar(200)
DECLARE @columnName varchar(200)
DECLARE @szQuery varchar(8000)
DECLARE @searchValue varchar(8000)
SET @szQuery = '';
SET @searchValue = '00000000-0000-0000-0000-000000000000';

DECLARE @lasttable varchar(255);
SET @lasttable='';

OPEN ABC

FETCH NEXT FROM abc INTO @tableName, @columnName
WHILE (@@FETCH_STATUS = 0)
BEGIN
    SET @szQuery = 
    	'SELECT '''+@tableName+''' AS TheTable, '''+@columnName+''' AS TheColumn '+
    	'FROM ['+@tableName+'] '+
    	'WHERE ['+@columnName+'] = '''+CAST(@searchValue AS varchar(50))+''''

    PRINT 'Searching '+@tableName+'.'+@columnName+'...'
    PRINT @szQuery
    EXEC (@szQuery)

    FETCH NEXT FROM abc INTO @tableName, @columnName
END

CLOSE abc
DEALLOCATE abc
