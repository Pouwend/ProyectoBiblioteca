/*
CREATE TABLE Autor (
    IdAutor INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Nacionalidad NVARCHAR(50)
);
GO
*/
INSERT INTO Autor (Nombre, Apellido, Nacionalidad) VALUES
('Gabriel', 'García Márquez', 'Colombiano'),
('Carlos', 'Ruiz Zafón', 'Español'),
('J.K.', 'Rowling', 'Británica'),
('Isabel', 'Allende', 'Chilena'),
('Julio', 'Cortázar', 'Argentino'),
('Yuval Noah', 'Harari', 'Israelí'),
('Miguel', 'de Cervantes', 'Español'),
('Suzanne', 'Collins', 'Estadounidense'),
('Mark', 'Manson', 'Estadounidense');
GO