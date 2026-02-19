SELECT name
FROM sys.tables
WHERE schema_id = SCHEMA_ID('cm')
ORDER BY name;
