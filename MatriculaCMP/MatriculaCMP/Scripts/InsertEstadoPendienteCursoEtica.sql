-- Script para insertar el estado "Pendiente de curso Ética" si no existe
-- Este estado (ID = 0) se usa para solicitudes de registro inicial que están esperando
-- que el médico complete el curso de ética antes de continuar con el proceso

-- Verificar si el estado ID=0 ya existe, si no, insertarlo
IF NOT EXISTS (SELECT 1 FROM EstadoSolicitudes WHERE Id = 0)
BEGIN
    SET IDENTITY_INSERT EstadoSolicitudes ON;
    
    INSERT INTO EstadoSolicitudes (Id, Nombre, Color, VerReporte)
    VALUES (0, 'Pendiente de curso Ética', 'warning', 1);
    
    SET IDENTITY_INSERT EstadoSolicitudes OFF;
    
    PRINT 'Estado "Pendiente de curso Ética" insertado correctamente.';
END
ELSE
BEGIN
    PRINT 'El estado "Pendiente de curso Ética" ya existe.';
END
GO

