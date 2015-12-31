-- this script renames a column in a table.
-- the column has ROWGUIDCOL property so can not be renamed with the table designer.
-- works on primary keys.

-- resources:
-- https://code.google.com/p/migratordotnet/issues/detail?id=43
-- http://stackoverflow.com/questions/16296622/rename-column-sql-server-2008

-- Ben Burns
-- Feb 28, 2014

ALTER TABLE [People.ContactInformation] ALTER COLUMN [ContactId] DROP ROWGUIDCOL;

EXEC sp_RENAME '[People.ContactInformation].ContactId', 'PersonId', 'COLUMN';

ALTER TABLE [People.ContactInformation] ALTER COLUMN [PersonId] ADD ROWGUIDCOL;
