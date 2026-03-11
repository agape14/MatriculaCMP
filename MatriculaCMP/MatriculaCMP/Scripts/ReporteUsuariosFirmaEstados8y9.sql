-- =============================================================================
-- DIAGNÓSTICO: Ejecutar primero para ver por qué no hay datos
-- =============================================================================

-- 1) ¿Hay registros en historial con estado 8 o 9?
SELECT 'Historial con EstadoNuevoId 8 o 9' AS Diagnostico, COUNT(*) AS Cantidad
FROM SolicitudHistorialEstados
WHERE EstadoNuevoId IN (8, 9);

-- 2) Esos registros: qué valor tiene UsuarioCambio y a qué usuario corresponde
SELECT h.Id, h.SolicitudId, h.EstadoNuevoId, h.UsuarioCambio, h.FechaCambio,
       TRY_CAST(h.UsuarioCambio AS INT) AS UsuarioIdInterpretado,
       u.Id AS UsuarioIdReal, u.NombreUsuario, u.PerfilId
FROM SolicitudHistorialEstados h
LEFT JOIN Usuarios u ON u.Id = TRY_CAST(h.UsuarioCambio AS INT)
WHERE h.EstadoNuevoId IN (8, 9)
ORDER BY h.FechaCambio DESC;

-- 3) Usuarios con PerfilId 3 o 4 (para verificar que existan)
SELECT Id, NombreUsuario, Correo, PerfilId FROM Usuarios WHERE PerfilId IN (3, 4);


-- =============================================================================
-- REPORTE PRINCIPAL: Usuarios que realizaron la firma (estado 8 o 9)
-- Usa JOIN por Id O por NombreUsuario (por si en algunos registros se guardó el nombre)
-- Solo usuarios con PerfilId IN (3, 4)
-- =============================================================================

SELECT
    u.Id AS UsuarioId,
    u.NombreUsuario,
    u.Correo,
    p.Id AS PerfilId,
    p.Nombre AS PerfilNombre,
    h.EstadoNuevoId,
    es.Nombre AS EstadoNombre,
    h.SolicitudId,
    s.NumeroSolicitud,
    h.FechaCambio,
    h.Observacion
FROM SolicitudHistorialEstados h
INNER JOIN EstadoSolicitudes es ON es.Id = h.EstadoNuevoId
INNER JOIN Solicitudes s ON s.Id = h.SolicitudId
INNER JOIN Usuarios u ON (u.Id = TRY_CAST(h.UsuarioCambio AS INT) OR (h.UsuarioCambio IS NOT NULL AND u.NombreUsuario = h.UsuarioCambio))
INNER JOIN Perfil p ON p.Id = u.PerfilId
WHERE h.EstadoNuevoId IN (8, 9)
  AND u.PerfilId IN (3, 4)
ORDER BY h.FechaCambio DESC, u.NombreUsuario, h.SolicitudId;


-- =============================================================================
-- REPORTE SIN FILTRO DE PERFIL (por si quiere ver todos los que firmaron)
-- =============================================================================
SELECT
    u.Id AS UsuarioId,
    u.NombreUsuario,
    u.Correo,
    p.Id AS PerfilId,
    p.Nombre AS PerfilNombre,
    h.EstadoNuevoId,
    es.Nombre AS EstadoNombre,
    h.SolicitudId,
    s.NumeroSolicitud,
    h.FechaCambio
FROM SolicitudHistorialEstados h
INNER JOIN EstadoSolicitudes es ON es.Id = h.EstadoNuevoId
INNER JOIN Solicitudes s ON s.Id = h.SolicitudId
LEFT JOIN Usuarios u ON (u.Id = TRY_CAST(h.UsuarioCambio AS INT) OR (h.UsuarioCambio IS NOT NULL AND u.NombreUsuario = h.UsuarioCambio))
LEFT JOIN Perfil p ON p.Id = u.PerfilId
WHERE h.EstadoNuevoId IN (8, 9)
ORDER BY h.FechaCambio DESC, h.SolicitudId;


-- =============================================================================
-- Resumen por usuario (cuántas veces firmó cada uno), PerfilId 3 y 4
-- =============================================================================
SELECT
    u.Id AS UsuarioId,
    u.NombreUsuario,
    u.Correo,
    p.Nombre AS PerfilNombre,
    h.EstadoNuevoId,
    es.Nombre AS EstadoNombre,
    COUNT(*) AS CantidadFirmas
FROM SolicitudHistorialEstados h
INNER JOIN EstadoSolicitudes es ON es.Id = h.EstadoNuevoId
INNER JOIN Usuarios u ON (u.Id = TRY_CAST(h.UsuarioCambio AS INT) OR (h.UsuarioCambio IS NOT NULL AND u.NombreUsuario = h.UsuarioCambio))
INNER JOIN Perfil p ON p.Id = u.PerfilId
WHERE h.EstadoNuevoId IN (8, 9)
  AND u.PerfilId IN (3, 4)
GROUP BY u.Id, u.NombreUsuario, u.Correo, p.Nombre, h.EstadoNuevoId, es.Nombre
ORDER BY CantidadFirmas DESC, u.NombreUsuario, h.EstadoNuevoId;
