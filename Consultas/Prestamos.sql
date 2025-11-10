/*
CREATE TABLE Prestamo (
    IdPrestamo INT PRIMARY KEY IDENTITY(1,1),
    IdUsuario INT NOT NULL,  -- Empleado que registra
    IdLector INT NOT NULL,   -- Lector que recibe
    IdEjemplar INT NOT NULL,
    FechaPrestamo DATETIME NOT NULL DEFAULT GETDATE(),
    FechaDevolucionEstimada DATETIME NOT NULL,
    FechaDevolucionReal DATETIME NULL,
    EstadoPrestamo NVARCHAR(20) DEFAULT 'Activo',
    Observaciones NVARCHAR(200),
    
    CONSTRAINT FK_Prestamo_Usuario FOREIGN KEY (IdUsuario) 
        REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Prestamo_Lector FOREIGN KEY (IdLector) 
        REFERENCES Lector(IdLector), 
    CONSTRAINT FK_Prestamo_Ejemplar FOREIGN KEY (IdEjemplar) 
        REFERENCES Ejemplares(IdEjemplar),
        
    CONSTRAINT CHK_Prestamo_Estado
        CHECK (EstadoPrestamo IN ('Activo', 'Devuelto', 'Vencido', 'Renovado'))
);
GO
*/
INSERT INTO Prestamo (IdUsuario, IdLector, IdEjemplar, FechaPrestamo, FechaDevolucionEstimada, FechaDevolucionReal, EstadoPrestamo) VALUES
-- Préstamos Devueltos (Ejemplares 1, 3, 6 ahora están 'Disponibles')
(1, 1, 1, '2025-10-15', '2025-10-22', '2025-10-21', 'Devuelto'),
(1, 3, 3, '2025-10-18', '2025-10-25', '2025-10-24', 'Devuelto'),
(2, 1, 6, '2025-10-25', '2025-11-01', '2025-10-31', 'Devuelto'),

-- Préstamo Activo (Ejemplar 5 está 'Prestado')
(2, 2, 5, '2025-11-04', '2025-11-11', NULL, 'Activo'), 

-- Préstamo Renovado (Ejemplar 15 está 'Prestado')
(3, 4, 15, '2025-10-22', '2025-11-05', NULL, 'Renovado'); 
GO