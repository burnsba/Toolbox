ALTER TABLE _table_name
ADD COLUMN Created DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
ADD COLUMN LastModified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;

DELIMITER //
CREATE TRIGGER _table_name_created BEFORE INSERT ON _table_name
FOR EACH ROW BEGIN
SET new.Created := now();
SET new.LastModified := now();
END;
//

CREATE TRIGGER _table_name_updated BEFORE UPDATE ON _table_name
FOR EACH ROW BEGIN
SET new.LastModified := now();
END;
//

DELIMITER ;

UPDATE _table_name SET Created = now();
UPDATE _table_name SET LastModified = now();
