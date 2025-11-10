/*
CREATE TABLE Lector (
    IdLector INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL,
    Direccion NVARCHAR(200),
    Telefono NVARCHAR(20),
    Email NVARCHAR(100),
    Edad INT,
    Carnet NVARCHAR(20) NULL,
    DUI NVARCHAR(10) NULL, 
    LimitePrestamos INT DEFAULT 3, 
    Estado NVARCHAR(20) DEFAULT 'Activo' 
        CONSTRAINT CHK_Lector_Estado CHECK (Estado IN ('Activo', 'Inactivo')),
    TipoUsuario NVARCHAR(20) NOT NULL,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT CHK_Lector_Identificacion 
        CHECK (Carnet IS NOT NULL OR DUI IS NOT NULL)
);
GO

CREATE UNIQUE INDEX UQ_Lector_Carnet_NoNulo
ON Lector(Carnet)
WHERE Carnet IS NOT NULL;
GO

CREATE UNIQUE INDEX UQ_Lector_DUI_NoNulo
ON Lector(DUI)
WHERE DUI IS NOT NULL;
GO
*/

INSERT INTO Lector (Nombre, Apellido, Direccion, Telefono, Email, Edad, Carnet, DUI, LimitePrestamos, TipoUsuario, Estado) VALUES
('Juan', 'Pérez García', 'San Salvador', '7890-1234', 'juan.perez@email.com', 20, 'PG2024001', '01234567-8', 3, 'Estudiante', 'Activo'),
('María', 'López Martínez', 'Santa Ana', '7123-4567', 'maria.lopez@email.com', 22, 'LM2024002', '02345678-9', 3, 'Estudiante', 'Activo'),
('Sandra', 'Morales Cruz', 'San Salvador', '7890-2345', 's.morales@email.com', 21, 'MS2024004', '03456789-0', 3, 'Estudiante', 'Activo'),
('Roberto', 'Castro Flores', 'Ahuachapán', '7345-6789', 'r.castro@email.com', 23, 'CR2024005', '06543210-9', 3, 'Estudiante', 'Inactivo'),
('Carlos', 'Hernández Rivas', 'San Miguel', '7456-7890', 'c.hernandez@email.com', 35, NULL, '09876543-2', 5, 'Docente', 'Activo'),
('Luis', 'González Mejía', 'La Libertad', '7567-8901', 'l.gonzalez@email.com', 40, NULL, '07654321-0', 5, 'Docente', 'Activo'),
('Ana', 'Ramírez Torres', 'Sonsonate', '7234-5678', 'ana.ramirez@email.com', 17, 'RA2024003', NULL, 3, 'Estudiante', 'Activo');
GO