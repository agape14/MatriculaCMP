-- =============================================================================
-- Script: Registrar 30 NUEVAS solicitudes de personas listas para firmar diplomas
-- Uso: Ejecutar en SQL Server sobre la base de datos de MatriculaCMP.
--
-- Qué hace:
--   1. Inserta 30 nuevas Personas (DNI de 8 dígitos: 20260205, 20260206, ..., 20260234).
--   2. Actualiza el correlativo y crea 30 nuevas Solicitudes en estado 6 (Sec. CR).
--   3. Asigna número de colegiatura a cada persona (formato CMP-YYYY-NNNNNN).
--   4. Inserta 30 registros en Diplomas (listos para /consejoregional/firmar-diplomas).
--
-- Requisitos:
--   - Debe existir GrupoSanguineoId = 177 en MaestroRegistro (valor por defecto de la app).
--   - Si no existe tabla Correlativos o está vacía, el script la inicializa.
--
-- Nota: El PDF del diploma se genera desde la aplicación (Regenerar diploma).
-- =============================================================================

-- -----------------------------------------------------------------------------
-- CORRECCIÓN: Ejecutar SOLO UNA VEZ para los registros ya insertados con S30_01_...
-- Convierte esos DNI a 8 dígitos: 20260205, 20260206, ..., 20260234.
-- Descomente el bloque, ejecútelo, y vuelva a comentarlo antes de usar el script de inserción.
-- -----------------------------------------------------------------------------
/*
UPDATE p
SET p.NumeroDocumento = CAST(20260205 + (x.rn - 1) AS VARCHAR(8))
FROM Personas p
INNER JOIN (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn
    FROM Personas
    WHERE NumeroDocumento LIKE N'S30[_]%'
) x ON x.Id = p.Id;
*/

SET NOCOUNT ON;

-- Sufijo temporal para identificar las 30 personas de esta ejecución (luego se corrigen a DNI 8 dígitos)
DECLARE @Sufijo NCHAR(9) = N'_' + CONVERT(VARCHAR(8), GETDATE(), 112);
DECLARE @BaseDni INT = CONVERT(INT, CONVERT(VARCHAR(8), GETDATE(), 112));  -- ej. 20260205

BEGIN TRANSACTION;

-- -----------------------------------------------------------------------------
-- 1) Asegurar correlativo para NumeroSolicitud (incrementar en 30)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Correlativos)
    INSERT INTO Correlativos (Nombre, UltimoNumero, FechaActualizacion)
    VALUES (N'SOLICITUD', 0, GETDATE());

DECLARE @StartNum INT;
UPDATE Correlativos
SET @StartNum = UltimoNumero,
    UltimoNumero = UltimoNumero + 30,
    FechaActualizacion = GETDATE()
WHERE Id = (SELECT MIN(Id) FROM Correlativos);

-- Si la tabla estaba vacía antes del INSERT, @StartNum puede ser NULL
IF @StartNum IS NULL SET @StartNum = 0;

-- -----------------------------------------------------------------------------
-- 2) Generar números del 1 al 30 (tabla de números)
-- -----------------------------------------------------------------------------
;WITH n AS (
    SELECT 1 AS num
    UNION ALL
    SELECT num + 1 FROM n WHERE num < 30
)
SELECT num INTO #Nums FROM n OPTION (MAXRECURSION 32);

-- -----------------------------------------------------------------------------
-- 3) Insertar 30 nuevas Personas (datos mínimos para diploma)
--    DNI: S30_01@sufijo .. S30_30@sufijo (sufijo = _YYYYMMDD, caben en 20 caracteres)
-- -----------------------------------------------------------------------------
INSERT INTO Personas (
    Nombres,
    ApellidoPaterno,
    ApellidoMaterno,
    NumeroDocumento,
    GrupoSanguineoId,
    FechaRegistro
)
SELECT
    N'Nombre' + RIGHT(N'0' + CAST(n.num AS VARCHAR(2)), 2),
    N'ApellidoP' + RIGHT(N'0' + CAST(n.num AS VARCHAR(2)), 2),
    N'ApellidoM' + RIGHT(N'0' + CAST(n.num AS VARCHAR(2)), 2),
    N'S30_' + RIGHT(N'0' + CAST(n.num AS VARCHAR(2)), 2) + @Sufijo,
    177,
    GETDATE()
FROM #Nums n;

-- -----------------------------------------------------------------------------
-- 4) Insertar 30 nuevas Solicitudes (estado 6 = listo para firma Sec. CR)
-- -----------------------------------------------------------------------------
INSERT INTO Solicitudes (
    PersonaId,
    TipoSolicitud,
    FechaSolicitud,
    EstadoSolicitudId,
    NumeroSolicitud,
    AceptaPoliticasPrivacidad,
    DDJJFirmadaIdPeru
)
SELECT
    p.Id,
    N'REGISTRO',
    GETDATE(),
    6,
    @StartNum + ROW_NUMBER() OVER (ORDER BY p.Id),
    1,
    0
FROM Personas p
WHERE p.NumeroDocumento LIKE N'S30[_]%' + @Sufijo
ORDER BY p.Id;

-- -----------------------------------------------------------------------------
-- 5) Asignar número de colegiatura a las 30 personas (formato CMP-YYYY-NNNNNN)
-- -----------------------------------------------------------------------------
UPDATE p
SET NumeroColegiatura = N'CMP-' + CAST(YEAR(GETDATE()) AS VARCHAR(4)) + N'-' + RIGHT(N'000000' + CAST(s.Id AS VARCHAR(10)), 6)
FROM Personas p
INNER JOIN Solicitudes s ON s.PersonaId = p.Id
WHERE p.NumeroDocumento LIKE N'S30[_]%' + @Sufijo;

-- -----------------------------------------------------------------------------
-- 6) Insertar 30 Diplomas (requerido para que aparezcan en firmar-diplomas)
-- -----------------------------------------------------------------------------
INSERT INTO Diplomas (
    SolicitudId,
    PersonaId,
    NombreCompleto,
    NumeroColegiatura,
    FechaEmision,
    RutaPdf,
    FechaCreacion,
    UsuarioCreacion
)
SELECT
    s.Id,
    s.PersonaId,
    RTRIM(p.Nombres + N' ' + p.ApellidoPaterno + ISNULL(N' ' + p.ApellidoMaterno, N'')),
    p.NumeroColegiatura,
    GETDATE(),
    NULL,
    GETDATE(),
    N'Script Preparar30RegistrosParaFirmarDiplomas'
FROM Solicitudes s
INNER JOIN Personas p ON p.Id = s.PersonaId
WHERE p.NumeroDocumento LIKE N'S30[_]%' + @Sufijo;

-- 6b) Corregir DNI a 8 dígitos: 20260205, 20260206, ..., 20260234 (para esta ejecución)
UPDATE p
SET p.NumeroDocumento = CAST(@BaseDni + (x.rn - 1) AS VARCHAR(8))
FROM Personas p
INNER JOIN (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn
    FROM Personas
    WHERE NumeroDocumento LIKE N'S30[_]%' + @Sufijo
) x ON x.Id = p.Id;

DROP TABLE #Nums;

COMMIT TRANSACTION;

-- -----------------------------------------------------------------------------
-- 7) Verificación: listar los 30 registros recién creados
-- -----------------------------------------------------------------------------
SELECT
    s.Id AS SolicitudId,
    s.NumeroSolicitud,
    s.EstadoSolicitudId,
    es.Nombre AS EstadoNombre,
    p.NumeroDocumento AS DNI,
    RTRIM(p.Nombres + N' ' + p.ApellidoPaterno + ISNULL(N' ' + p.ApellidoMaterno, N'')) AS NombreCompleto,
    p.NumeroColegiatura,
    d.FechaEmision AS DiplomaFechaEmision,
    d.RutaPdf
FROM Solicitudes s
INNER JOIN EstadoSolicitudes es ON es.Id = s.EstadoSolicitudId
INNER JOIN Personas p ON p.Id = s.PersonaId
INNER JOIN Diplomas d ON d.SolicitudId = s.Id
WHERE p.NumeroDocumento >= CAST(@BaseDni AS VARCHAR(8))
  AND p.NumeroDocumento <= CAST(@BaseDni + 29 AS VARCHAR(8))
ORDER BY s.NumeroSolicitud;

SET NOCOUNT OFF;
