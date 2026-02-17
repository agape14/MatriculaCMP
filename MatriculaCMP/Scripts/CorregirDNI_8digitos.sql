-- =============================================================================
-- Corregir DNI: de S30_01_20260205, etc. a 8 dígitos (20260205, 20260206, ...)
-- Ejecutar UNA SOLA VEZ sobre los registros ya insertados por el script de 30 diplomas.
-- =============================================================================

UPDATE p
SET p.NumeroDocumento = CAST(20260205 + (x.rn - 1) AS VARCHAR(8))
FROM Personas p
INNER JOIN (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn
    FROM Personas
    WHERE NumeroDocumento LIKE N'S30[_]%'
) x ON x.Id = p.Id;

-- Verificación
SELECT Id, NumeroDocumento, Nombres, ApellidoPaterno
FROM Personas
WHERE NumeroDocumento LIKE N'202602%'
ORDER BY NumeroDocumento;
