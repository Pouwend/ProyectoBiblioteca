/*
CREATE TABLE GeneroLibro (
    IdGeneroLibro INT PRIMARY KEY IDENTITY(1,1),
    IdGenero INT NOT NULL,
    IdLibro INT NOT NULL,
    CONSTRAINT FK_GeneroLibro_Genero FOREIGN KEY (IdGenero) REFERENCES Genero(IdGenero),
    CONSTRAINT FK_GeneroLibro_Libro FOREIGN KEY (IdLibro) REFERENCES Libros(IdLibros),
    CONSTRAINT UQ_GeneroLibro UNIQUE (IdGenero, IdLibro)
);
GO
*/
INSERT INTO GeneroLibro (IdGenero, IdLibro) VALUES
(1, 1), (8, 1), -- (Ficción, Novela: Cien Años...)
(2, 2), -- (Misterio: La Sombra...)
(3, 3), -- (Fantasía: Harry Potter)
(1, 4), -- (Ficción: La Casa...)
(1, 5), (8, 5), -- (Ficción, Novela: Rayuela)
(5, 6), -- (Ensayo: Sapiens)
(6, 7), -- (Literatura: Don Quijote)
(4, 8), -- (Ciencia Ficción: Los Juegos...)
(7, 9); -- (Autoayuda: El Sutil Arte...)
GO