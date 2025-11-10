/*
CREATE TABLE Ejemplares (
    IdEjemplar INT PRIMARY KEY IDENTITY(1,1),
    IdLibro INT NOT NULL,
    CodigoEjemplar NVARCHAR(20) NOT NULL UNIQUE,
    EstadoEjemplar NVARCHAR(20) NOT NULL DEFAULT 'Disponible',
    FechaAdquisicion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Ejemplares_Libros FOREIGN KEY (IdLibro) 
        REFERENCES Libros(IdLibros),

    CONSTRAINT CHK_Ejemplares_Estado 
        CHECK (EstadoEjemplar IN ('Disponible', 'Prestado', 'Dañado', 'Perdido'))
);
GO
*/
INSERT INTO Ejemplares (IdLibro, CodigoEjemplar, EstadoEjemplar) VALUES
(1, 'EJ-001-001', 'Disponible'), -- IdEjemplar 1
(1, 'EJ-001-002', 'Disponible'), -- IdEjemplar 2
(1, 'EJ-001-003', 'Disponible'), -- IdEjemplar 3
(2, 'EJ-002-001', 'Disponible'), -- IdEjemplar 4
(2, 'EJ-002-002', 'Prestado'),   -- IdEjemplar 5 
(3, 'EJ-003-001', 'Disponible'), -- IdEjemplar 6
(3, 'EJ-003-002', 'Disponible'), -- IdEjemplar 7
(4, 'EJ-004-001', 'Disponible'), -- IdEjemplar 8
(4, 'EJ-004-002', 'Dañado'),     -- IdEjemplar 9
(5, 'EJ-005-001', 'Disponible'), -- IdEjemplar 10
(6, 'EJ-006-001', 'Disponible'), -- IdEjemplar 11
(6, 'EJ-006-002', 'Disponible'), -- IdEjemplar 12
(7, 'EJ-007-001', 'Disponible'), -- IdEjemplar 13
(7, 'EJ-007-002', 'Disponible'), -- IdEjemplar 14
(7, 'EJ-007-003', 'Prestado');   -- IdEjemplar 15 
GO