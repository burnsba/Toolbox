USE _database_name;

ALTER TABLE dbo._table_name ADD Created DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
GO

ALTER TABLE dbo._table_name ADD LastModified DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
GO

CREATE TRIGGER dbo.SetLastModified_table_name
ON dbo._table_name 
AFTER UPDATE
AS
BEGIN
    IF NOT UPDATE(LastModified)
    BEGIN
        UPDATE t
            SET t.LastModified = CURRENT_TIMESTAMP
            FROM dbo._table_name AS t
            INNER JOIN inserted AS i 
            ON t.ID = i.ID;
    END
END
GO
