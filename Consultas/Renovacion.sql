/*
CREATE TABLE Renovacion (
    IdRenovacion INT PRIMARY KEY IDENTITY(1,1),
    IdPrestamo INT NOT NULL,
    FechaRenovacion DATETIME DEFAULT GETDATE(),
    FechaAnteriorDevolucion DATETIME NOT NULL,
    FechaNuevaDevolucion DATETIME NOT NULL,
    RenovadoAnterior BIT DEFAULT 0, -- 
    
    CONSTRAINT FK_Renovacion_Prestamo FOREIGN KEY (IdPrestamo) 
        REFERENCES Prestamo(IdPrestamo),

    CONSTRAINT UK_Renovacion_Prestamo UNIQUE (IdPrestamo) 
);
GO
*/
INSERT INTO Renovacion (IdPrestamo, FechaRenovacion, FechaAnteriorDevolucion, FechaNuevaDevolucion)
VALUES (5, '2025-10-28 10:00:00', '2025-10-29', '2025-11-05');
GO