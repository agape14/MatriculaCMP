-- =============================================================================
-- Inserción completa de la tabla [Menu] (tras vaciarla) y asignación en [PerfilMenu].
-- Ejecutar con la tabla Menu vacía. Para ver los menús, asignar ítems a perfiles en
-- PerfilMenu (ej. PerfilId = 1 para Administrador).
-- =============================================================================

SET IDENTITY_INSERT [dbo].[Menu] ON;

INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (1, N'Reg. Pre-matricula', N'/solicitudes/prematricula', N'SOLICITANTE', N'ri-file-add-line', N'ri-user-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (2, N'Seguimiento Trámite', N'/solicitudes/seguimiento', N'SOLICITANTE', N'ri-timer-line', N'ri-user-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (3, N'Solicitudes', N'/consejoregional/lista', N'Consejo Regional', N'ri-folder-shared-line', N'ri-government-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (4, N'Firmar Diplomas', N'/consejoregional/firmar-diplomas', N'Consejo Regional', N'ri-shield-keyhole-line', N'ri-government-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (5, N'Firmar Diplomas (Sec. CR)', N'/consejoregional/firmar-diplomas/sec', N'Consejo Regional', N'ri-user-2-line', NULL, NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (6, N'Firmar Diplomas (Decano CR)', N'/consejoregional/firmar-diplomas/decano', N'Consejo Regional', N'ri-award-line', NULL, NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (7, N'Ver diplomas firmados', N'/consejoregional/diplomas-firmados', N'Consejo Regional', N'ri-file-list-3-line', NULL, NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (8, N'Firmar Diplomas', N'/secretariageneral/firmar-diplomas', N'Secretaría General', N'ri-shield-keyhole-line', N'ri-shield-star-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (9, N'Lista Autorizadas', N'/oficinamatricula/lista', N'Oficina Matrícula', N'ri-check-double-line', N'ri-building-line ', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (10, N'Firmar Diplomas', N'/decanato/lista', N'Decanato', N'ri-file-list-3-line', N'ri-award-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (11, N'Usuarios', N'/admin/usuarios', N'Administrador', N'ri-user-add-line', N'ri-settings-3-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (12, N'Perfil', N'/admin/perfiles', N'Administrador', N'ri-user-settings-line', N'ri-settings-3-line', NULL);
INSERT INTO [dbo].[Menu] ([Id], [Titulo], [Ruta], [Modulo], [Icono], [ModuloIcono], [Orden]) VALUES (13, N'Menú Perfiles', N'/admin/perfiles-menu', N'Administrador', N'ri-menu-add-line', N'ri-settings-3-line', NULL);

SET IDENTITY_INSERT [dbo].[Menu] OFF;

-- =============================================================================
-- PERFILMENU: asignar menús al perfil Administrador (Id = 1) para que se vean en la app.
-- Ajustar @PerfilId si tu usuario administrador tiene otro perfil.
-- =============================================================================

DECLARE @PerfilId INT = 1;

INSERT INTO [dbo].[PerfilMenu] ([PerfilId], [MenuId])
SELECT @PerfilId, m.[Id]
FROM [dbo].[Menu] m
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[PerfilMenu] pm
    WHERE pm.[PerfilId] = @PerfilId AND pm.[MenuId] = m.[Id]
);
