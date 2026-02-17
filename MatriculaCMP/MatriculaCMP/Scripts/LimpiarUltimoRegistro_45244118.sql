-- Script para limpiar el último registro de JUNIOR ELVIS GARGATE CHINCHAY (DNI: 45244118)
-- PersonaId: 22
-- IMPORTANTE: Ejecutar dentro de una transacción para poder revertir si es necesario

BEGIN TRANSACTION;

DECLARE @PersonaId INT = 22;
DECLARE @NumeroDocumento NVARCHAR(20) = '45244118';
DECLARE @UltimaSolicitudId INT;

-- Verificar que la persona existe
IF NOT EXISTS (SELECT 1 FROM Personas WHERE Id = @PersonaId AND NumeroDocumento = @NumeroDocumento)
BEGIN
    PRINT 'ERROR: No se encontró la persona con Id ' + CAST(@PersonaId AS NVARCHAR) + ' y DNI ' + @NumeroDocumento;
    ROLLBACK TRANSACTION;
    RETURN;
END

-- Obtener el ID de la última solicitud de esta persona
SELECT TOP 1 @UltimaSolicitudId = Id 
FROM Solicitudes 
WHERE PersonaId = @PersonaId 
ORDER BY FechaSolicitud DESC, Id DESC;

IF @UltimaSolicitudId IS NULL
BEGIN
    PRINT 'No se encontraron solicitudes para esta persona.';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT 'Última solicitud encontrada: ' + CAST(@UltimaSolicitudId AS NVARCHAR);
PRINT 'Estado de la solicitud: ' + CAST((SELECT EstadoSolicitudId FROM Solicitudes WHERE Id = @UltimaSolicitudId) AS NVARCHAR);

-- 1. Eliminar el historial de estados de esta solicitud
DELETE FROM SolicitudHistorialEstados
WHERE SolicitudId = @UltimaSolicitudId;
PRINT 'Registros eliminados de SolicitudHistorialEstados: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- 2. Eliminar la educación asociada a esta solicitud (si existe)
DELETE FROM Educacion
WHERE PersonaId = @PersonaId 
  AND Id = (SELECT EducacionId FROM Solicitudes WHERE Id = @UltimaSolicitudId);
PRINT 'Registros eliminados de Educacion: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- 3. Eliminar la solicitud
DELETE FROM Solicitudes
WHERE Id = @UltimaSolicitudId;
PRINT 'Solicitud eliminada: ' + CAST(@@ROWCOUNT AS NVARCHAR);

-- Mostrar resumen
PRINT '---------------------------------------------------';
PRINT 'RESUMEN DE LA LIMPIEZA:';
PRINT 'PersonaId: ' + CAST(@PersonaId AS NVARCHAR);
PRINT 'Documento: ' + @NumeroDocumento;
PRINT 'SolicitudId eliminada: ' + CAST(@UltimaSolicitudId AS NVARCHAR);
PRINT '---------------------------------------------------';
PRINT 'Si todo es correcto, ejecute: COMMIT TRANSACTION';
PRINT 'Si desea revertir los cambios, ejecute: ROLLBACK TRANSACTION';

-- DESCOMENTAR UNA DE LAS SIGUIENTES LÍNEAS:
-- COMMIT TRANSACTION;    -- Para confirmar los cambios
ROLLBACK TRANSACTION;  -- Para revertir (configuración segura por defecto)

