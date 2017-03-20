-- List all triggers

SELECT 
    t.[name] AS [Trigger],
    s.[name] AS [Table], 
    asm.[definition] AS [Definition]
FROM [sys].all_sql_modules AS asm
JOIN [sys].triggers AS t ON asm.object_id = t.object_id
JOIN sysobjects AS s ON t.parent_id = s.id
