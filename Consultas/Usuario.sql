/*
CREATE TABLE Usuario (
    IdUsuario INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL,
    NombreUsuario NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(200) NOT NULL,
    Rol NVARCHAR(30) NOT NULL DEFAULT 'Bibliotecario',
    DUI NVARCHAR(10) UNIQUE NOT NULL
);
GO
*/
INSERT INTO Usuario (Nombre,NombreUsuario, Password, Rol, DUI) VALUES
('Juan Perez','jperez.01', 'BB2025', 'Bibliotecario', '12345678-9'),
('María Lopez','mlopez.02', 'BB2025', 'Bibliotecario', '98765432-1'),
('Manuel Chavez','mchavez.03', 'BB2025', 'Bibliotecario', '45678912-3');
GO
