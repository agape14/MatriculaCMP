-- Script para insertar estado intermedio "Curso completado, pendiente de documentos"
-- Este estado se usa cuando el usuario completa el curso de ética pero aún no ha enviado el formulario completo

SET IDENTITY_INSERT [dbo].[EstadoSolicitudes] ON;

-- Verificar si el estado -1 ya existe
IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadoSolicitudes] WHERE [Id] = -1)
BEGIN
    INSERT INTO [dbo].[EstadoSolicitudes] ([Id], [Nombre], [Color])
    VALUES (-1, 'Curso completado, pendiente de documentos', 'info');
    
    PRINT 'Estado -1 "Curso completado, pendiente de documentos" insertado correctamente.';
END
ELSE
BEGIN
    PRINT 'El estado -1 ya existe en la base de datos.';
END

SET IDENTITY_INSERT [dbo].[EstadoSolicitudes] OFF;

