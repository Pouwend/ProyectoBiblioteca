/*
CREATE TABLE Libros (
    IdLibros INT PRIMARY KEY IDENTITY(1,1),
    ISBN NVARCHAR(20) NOT NULL UNIQUE,
    Titulo NVARCHAR(200) NOT NULL,
    Editorial NVARCHAR(100) NOT NULL,
    AnioPublicacion INT NOT NULL,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    Estado BIT DEFAULT 1 -- 1= Activo, 0= Inactivo
);
GO
*/
INSERT INTO Libros (ISBN, Titulo, Editorial, AnioPublicacion) VALUES
('978-0307474728', 'Cien Años de Soledad', 'Sudamericana', 1967),
('978-8497592208', 'La Sombra del Viento', 'Planeta', 2001),   
('978-0747532699', 'Harry Potter y la Piedra Filosofal', 'Salamandra', 1997),
('978-1501167638', 'La Casa de los Espíritus', 'Plaza & Janés', 1982),
('978-0374530341', 'Rayuela', 'Sudamericana', 1963),                
('978-8499926127', 'Sapiens: De animales a dioses', 'Debate', 2011),
('978-0060934347', 'Don Quijote de la Mancha', 'Francisco de Robles', 1605),
('978-0439023481', 'Los Juegos del Hambre', 'Scholastic Press', 2008),     
('978-0062457714', 'El Sutil Arte de que te Importe un C*rajo', 'Harper', 2016);
GO