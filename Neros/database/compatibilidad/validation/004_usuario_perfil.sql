IF COL_LENGTH('dbo.AspNetUsers', 'PrimerNombre') IS NULL THROW 50040, 'Falta AspNetUsers.PrimerNombre.', 1;
IF COL_LENGTH('dbo.UsuariosEmpresas', 'ModulosHabilitados') IS NULL THROW 50041, 'Falta UsuariosEmpresas.ModulosHabilitados.', 1;
