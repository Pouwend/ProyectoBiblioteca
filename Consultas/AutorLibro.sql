/*
CREATE TABLE AutorLibro (
    IdAutorLibro INT PRIMARY KEY IDENTITY(1,1),
    IdAutor INT NOT NULL,
    IdLibro INT NOT NULL,
    CONSTRAINT FK_AutorLibro_Autor FOREIGN KEY (IdAutor) REFERENCES Autor(IdAutor),
    CONSTRAINT FK_AutorLiblO_Libro FOREIGN KEY (IdLibro) REFERENCES Libros(IdLibros),
    CONSTRAINT UQ_AutorLibro UNIQUE (IdAutor, IdLibro)
);
GO
*/

INSERT INTO AutorLibro (IdAutor, IdLibro) VALUES
(1, 1), -- (García Márquez, Cien Años...)
(2, 2), -- (Ruiz Zafón, La Sombra...)
(3, 3), -- (Rowling, Harry Potter)
(4, 4), -- (Allende, La Casa...)
(5, 5), -- (Cortázar, Rayuela)
(6, 6), -- (Harari, Sapiens)
(7, 7), -- (Cervantes, Don Quijote)
(8, 8), -- (Collins, Los Juegos...)
(9, 9); -- (Manson, El Sutil Arte...)
GO