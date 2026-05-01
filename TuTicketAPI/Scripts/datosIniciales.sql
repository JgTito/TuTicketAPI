USE TicketManagerDB;
GO

/* ============================================================
   ROLES (Identity)
   ============================================================ */

IF OBJECT_ID('AspNetRoles', 'U') IS NOT NULL
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    SELECT NEWID(), 'Administrador', 'ADMINISTRADOR', NEWID()
    WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'ADMINISTRADOR');

    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    SELECT NEWID(), 'EncargadoCategoria', 'ENCARGADOCATEGORIA', NEWID()
    WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'ENCARGADOCATEGORIA');

    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    SELECT NEWID(), 'ResolvedorTicket', 'RESOLVEDORTICKET', NEWID()
    WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'RESOLVEDORTICKET');
END;
GO

/* ============================================================
   ESTADOS
   ============================================================ */

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'Abierto', 0, 1 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'Abierto');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'Pendiente de derivación', 0, 2 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'Pendiente de derivación');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'Derivado', 0, 3 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'Derivado');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'En análisis', 0, 4 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'En análisis');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'En proceso', 0, 5 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'En proceso');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'En espera', 0, 6 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'En espera');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'Resuelto', 0, 7 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'Resuelto');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'Reabierto', 0, 8 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'Reabierto');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'Cerrado', 1, 9 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'Cerrado');

INSERT INTO EstadoTicket (Nombre, EsEstadoFinal, Orden)
SELECT 'Cancelado', 1, 10 WHERE NOT EXISTS (SELECT 1 FROM EstadoTicket WHERE Nombre = 'Cancelado');
GO

/* ============================================================
   PRIORIDADES
   ============================================================ */

INSERT INTO PrioridadTicket (Nombre, Nivel)
SELECT 'Baja', 1 WHERE NOT EXISTS (SELECT 1 FROM PrioridadTicket WHERE Nombre = 'Baja');

INSERT INTO PrioridadTicket (Nombre, Nivel)
SELECT 'Media', 2 WHERE NOT EXISTS (SELECT 1 FROM PrioridadTicket WHERE Nombre = 'Media');

INSERT INTO PrioridadTicket (Nombre, Nivel)
SELECT 'Alta', 3 WHERE NOT EXISTS (SELECT 1 FROM PrioridadTicket WHERE Nombre = 'Alta');

INSERT INTO PrioridadTicket (Nombre, Nivel)
SELECT 'Crítica', 4 WHERE NOT EXISTS (SELECT 1 FROM PrioridadTicket WHERE Nombre = 'Crítica');
GO

/* ============================================================
   CATEGORÍAS
   ============================================================ */

INSERT INTO CategoriaTicket (Nombre)
SELECT 'Software' WHERE NOT EXISTS (SELECT 1 FROM CategoriaTicket WHERE Nombre = 'Software');

INSERT INTO CategoriaTicket (Nombre)
SELECT 'Infraestructura' WHERE NOT EXISTS (SELECT 1 FROM CategoriaTicket WHERE Nombre = 'Infraestructura');

INSERT INTO CategoriaTicket (Nombre)
SELECT 'Accesos' WHERE NOT EXISTS (SELECT 1 FROM CategoriaTicket WHERE Nombre = 'Accesos');
GO

/* ============================================================
   SUBCATEGORÍAS
   ============================================================ */

INSERT INTO SubcategoriaTicket (IdCategoriaTicket, Nombre)
SELECT c.IdCategoriaTicket, 'Error sistema'
FROM CategoriaTicket c
WHERE c.Nombre = 'Software'
AND NOT EXISTS (
    SELECT 1 FROM SubcategoriaTicket 
    WHERE Nombre = 'Error sistema'
);

INSERT INTO SubcategoriaTicket (IdCategoriaTicket, Nombre)
SELECT c.IdCategoriaTicket, 'Nueva funcionalidad'
FROM CategoriaTicket c
WHERE c.Nombre = 'Software'
AND NOT EXISTS (
    SELECT 1 FROM SubcategoriaTicket 
    WHERE Nombre = 'Nueva funcionalidad'
);

INSERT INTO SubcategoriaTicket (IdCategoriaTicket, Nombre)
SELECT c.IdCategoriaTicket, 'Servidor caído'
FROM CategoriaTicket c
WHERE c.Nombre = 'Infraestructura'
AND NOT EXISTS (
    SELECT 1 FROM SubcategoriaTicket 
    WHERE Nombre = 'Servidor caído'
);

INSERT INTO SubcategoriaTicket (IdCategoriaTicket, Nombre)
SELECT c.IdCategoriaTicket, 'Problema de red'
FROM CategoriaTicket c
WHERE c.Nombre = 'Infraestructura'
AND NOT EXISTS (
    SELECT 1 FROM SubcategoriaTicket 
    WHERE Nombre = 'Problema de red'
);

INSERT INTO SubcategoriaTicket (IdCategoriaTicket, Nombre)
SELECT c.IdCategoriaTicket, 'Creación usuario'
FROM CategoriaTicket c
WHERE c.Nombre = 'Accesos'
AND NOT EXISTS (
    SELECT 1 FROM SubcategoriaTicket 
    WHERE Nombre = 'Creación usuario'
);
GO

/* ============================================================
   EQUIPOS
   ============================================================ */

INSERT INTO EquipoSoporte (Nombre)
SELECT 'Desarrollo' WHERE NOT EXISTS (SELECT 1 FROM EquipoSoporte WHERE Nombre = 'Desarrollo');

INSERT INTO EquipoSoporte (Nombre)
SELECT 'Infraestructura' WHERE NOT EXISTS (SELECT 1 FROM EquipoSoporte WHERE Nombre = 'Infraestructura');

INSERT INTO EquipoSoporte (Nombre)
SELECT 'Mesa de Ayuda' WHERE NOT EXISTS (SELECT 1 FROM EquipoSoporte WHERE Nombre = 'Mesa de Ayuda');
GO

/* ============================================================
   CATEGORIA - EQUIPO
   ============================================================ */

INSERT INTO CategoriaEquipoSoporte (IdCategoriaTicket, IdEquipoSoporte)
SELECT c.IdCategoriaTicket, e.IdEquipoSoporte
FROM CategoriaTicket c
JOIN EquipoSoporte e ON e.Nombre = 'Desarrollo'
WHERE c.Nombre = 'Software'
AND NOT EXISTS (
    SELECT 1 FROM CategoriaEquipoSoporte
    WHERE IdCategoriaTicket = c.IdCategoriaTicket
    AND IdEquipoSoporte = e.IdEquipoSoporte
);

INSERT INTO CategoriaEquipoSoporte (IdCategoriaTicket, IdEquipoSoporte)
SELECT c.IdCategoriaTicket, e.IdEquipoSoporte
FROM CategoriaTicket c
JOIN EquipoSoporte e ON e.Nombre = 'Infraestructura'
WHERE c.Nombre = 'Infraestructura'
AND NOT EXISTS (
    SELECT 1 FROM CategoriaEquipoSoporte
    WHERE IdCategoriaTicket = c.IdCategoriaTicket
    AND IdEquipoSoporte = e.IdEquipoSoporte
);
GO

/* ============================================================
   SLA
   ============================================================ */

INSERT INTO SlaPolitica (Nombre)
SELECT 'SLA General' WHERE NOT EXISTS (SELECT 1 FROM SlaPolitica WHERE Nombre = 'SLA General');

DECLARE @SlaId INT = (SELECT IdSlaPolitica FROM SlaPolitica WHERE Nombre = 'SLA General');

INSERT INTO SlaRegla (IdSlaPolitica, IdPrioridadTicket, MinutosPrimeraRespuesta, MinutosResolucion)
SELECT @SlaId, p.IdPrioridadTicket, 1440, 7200
FROM PrioridadTicket p
WHERE p.Nombre = 'Baja'
AND NOT EXISTS (
    SELECT 1 FROM SlaRegla WHERE IdPrioridadTicket = p.IdPrioridadTicket
);

INSERT INTO SlaRegla (IdSlaPolitica, IdPrioridadTicket, MinutosPrimeraRespuesta, MinutosResolucion)
SELECT @SlaId, p.IdPrioridadTicket, 480, 4320
FROM PrioridadTicket p
WHERE p.Nombre = 'Media'
AND NOT EXISTS (
    SELECT 1 FROM SlaRegla WHERE IdPrioridadTicket = p.IdPrioridadTicket
);

INSERT INTO SlaRegla (IdSlaPolitica, IdPrioridadTicket, MinutosPrimeraRespuesta, MinutosResolucion)
SELECT @SlaId, p.IdPrioridadTicket, 120, 1440
FROM PrioridadTicket p
WHERE p.Nombre = 'Alta'
AND NOT EXISTS (
    SELECT 1 FROM SlaRegla WHERE IdPrioridadTicket = p.IdPrioridadTicket
);

INSERT INTO SlaRegla (IdSlaPolitica, IdPrioridadTicket, MinutosPrimeraRespuesta, MinutosResolucion)
SELECT @SlaId, p.IdPrioridadTicket, 30, 240
FROM PrioridadTicket p
WHERE p.Nombre = 'Crítica'
AND NOT EXISTS (
    SELECT 1 FROM SlaRegla WHERE IdPrioridadTicket = p.IdPrioridadTicket
);
GO