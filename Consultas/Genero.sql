/*
CREATE TABLE Genero (
    IdGenero INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(200)
);
GO
*/
INSERT INTO Genero (Nombre, Descripcion) VALUES
('Ficción', 'Narrativa de eventos imaginarios'),
('Misterio', 'Historias de suspenso y enigmas'),
('Fantasía', 'Mundos imaginarios y mágicos'),
('Ciencia Ficción', 'Ficción basada en ciencia y tecnología'),
('Ensayo', 'Texto argumentativo sobre un tema'),
('Literatura', 'Obras clásicas de valor artístico'),
('Autoayuda', 'Desarrollo personal'),
('Novela', 'Narrativa extensa de ficción');
GO