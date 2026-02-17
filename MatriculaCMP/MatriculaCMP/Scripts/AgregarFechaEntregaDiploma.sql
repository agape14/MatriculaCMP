-- =============================================
-- Script SQL para agregar el campo FechaEntrega a la tabla Diplomas
-- Ejecutar este script en SQL Server Management Studio
-- =============================================

-- IMPORTANTE: Reemplazar [NombreDeTuBaseDeDatos] con el nombre real de tu base de datos
-- O simplemente seleccionar la base de datos correcta antes de ejecutar

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Diplomas]') 
    AND name = 'FechaEntrega'
)
BEGIN
    -- Agregar la columna FechaEntrega como nullable (DateTime?)
    ALTER TABLE [dbo].[Diplomas]
    ADD [FechaEntrega] DATETIME2 NULL;
    
    PRINT 'Columna FechaEntrega agregada exitosamente a la tabla Diplomas';
END
ELSE
BEGIN
    PRINT 'La columna FechaEntrega ya existe en la tabla Diplomas';
END
GO

-- Verificar que la columna se agregó correctamente
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Diplomas' 
    AND COLUMN_NAME = 'FechaEntrega';
GO
