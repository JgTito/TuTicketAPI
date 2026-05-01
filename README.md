# TuTicketAPI

API REST para gestion de tickets de soporte. Incluye autenticacion JWT con ASP.NET Core Identity, mantenedores de catalogos, configuracion de responsables/equipos, ciclo de vida de tickets, SLA, adjuntos, bitacora, relaciones, notificaciones y endpoints para graficos.

## Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- AutoMapper
- Swagger / OpenAPI

## Estructura principal

```text
TuTicketAPI/
  Authorization/       Roles de la aplicacion
  Constants/           Constantes de dominio
  Controllers/         Endpoints REST
  Dtos/                Contratos de entrada/salida
  Enums/               Enumeraciones de dominio
  Mappings/            Perfiles AutoMapper
  Migrations/          Migraciones EF Core
  Models/              Entidades y DbContext
  Services/            Servicios de dominio y soporte
```

## Requisitos

- .NET SDK 9
- SQL Server o SQL Server Express
- Base de datos `TuTicket`
- Roles Identity creados:
  - `Administrador`
  - `ResolvedorTicket`
  - `Solicitante`

## Configuracion

La cadena de conexion esta en `TuTicketAPI/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS01;Database=TuTicket;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

La clave JWT debe configurarse en `TuTicketAPI/appsettings.Development.json` o mediante variables de entorno:

```json
"Jwt": {
  "Key": "TuTicketAPI-Key",
  "Issuer": "TuTicketAPI",
  "Audience": "TuTicketAPI.Client",
  "ExpirationMinutes": 120
}
```

## Ejecucion local

Restaurar y compilar:

```powershell
dotnet restore TuTicketAPI.sln
dotnet build TuTicketAPI.sln
```

Ejecutar API:

```powershell
dotnet run --project TuTicketAPI\TuTicketAPI.csproj
```

URLs por defecto:

- HTTP: `http://localhost:5151`
- HTTPS: `https://localhost:7113`
- Swagger: `https://localhost:7113/swagger`

## Base de datos y migraciones

Crear o actualizar la base:

```powershell
dotnet ef database update --project TuTicketAPI\TuTicketAPI.csproj --startup-project TuTicketAPI\TuTicketAPI.csproj
```

Crear una nueva migracion:

```powershell
dotnet ef migrations add NombreMigracion --project TuTicketAPI\TuTicketAPI.csproj --startup-project TuTicketAPI\TuTicketAPI.csproj
```

## Datos iniciales

El proyecto incluye un script SQL para cargar datos base:

```text
TuTicketAPI/Scripts/datosIniciales.sql
```

Antes de ejecutarlo, revisar la primera linea del script:

```sql
USE TicketManagerDB;
```

Si la base local se llama `TuTicket`, cambiar esa linea por:

```sql
USE TuTicket;
```

El script inserta datos solo si no existen previamente. Carga:

- Roles Identity:
  - `Administrador`
  - `EncargadoCategoria`
  - `ResolvedorTicket`
- Estados de ticket:
  - `Abierto`
  - `Pendiente de derivación`
  - `Derivado`
  - `En análisis`
  - `En proceso`
  - `En espera`
  - `Resuelto`
  - `Reabierto`
  - `Cerrado`
  - `Cancelado`
- Prioridades:
  - `Baja`
  - `Media`
  - `Alta`
  - `Crítica`
- Categorias:
  - `Software`
  - `Infraestructura`
  - `Accesos`
- Subcategorias iniciales para cada categoria.
- Equipos de soporte:
  - `Desarrollo`
  - `Infraestructura`
  - `Mesa de Ayuda`
- Relacion categoria/equipo.
- Politica `SLA General` y reglas SLA por prioridad.

## Autenticacion

La API usa JWT Bearer. Para consumir endpoints protegidos, enviar:

```http
Authorization: Bearer <token>
```

Endpoints principales:

- `POST /api/Usuario/registrar`
  - Crea usuario con rol `Solicitante` por defecto.
- `POST /api/Usuario/login`
  - Devuelve token JWT, datos de usuario y roles.
- `GET /api/Usuario/select`
  - Lista usuarios para selects.

## Roles

- `Administrador`
  - Acceso completo a mantenedores, tickets, notificaciones y configuracion.
- `ResolvedorTicket`
  - Acceso a tickets asignados y tickets permitidos por configuracion de categoria/equipo soporte.
- `Solicitante`
  - Acceso a sus propios tickets y acciones permitidas sobre ellos.

## Modulos y endpoints

### Tickets

Controller: `TicketController`

Endpoints relevantes:

- `GET /api/Ticket`
  - Lista paginada con filtros por estado, prioridad, subcategoria, usuarios, texto y rangos de fechas.
- `GET /api/Ticket/{id}`
- `POST /api/Ticket`
  - Crea ticket, asigna solicitante desde usuario logeado y responsable desde la categoria.
  - Permite adjuntos iniciales.
  - Crea SLA, historial y notificaciones.
- `PUT /api/Ticket/{id}`
- `PUT /api/Ticket/{id}/asignar`
- `PUT /api/Ticket/{id}/cambiar-estado`
- `PUT /api/Ticket/{id}/cambiar-prioridad`
- `GET /api/Ticket/{id}/historial`
- `GET /api/Ticket/{id}/estados-disponibles`

### Adjuntos

Controller: `TicketAdjuntoController`

- `GET /api/Ticket/{idTicket}/adjuntos`
- `POST /api/Ticket/{idTicket}/adjuntos`
  - Permite subir lista de archivos.
- `GET /api/TicketAdjunto/{id}`
- `GET /api/TicketAdjunto/{id}/descargar`
- `DELETE /api/TicketAdjunto/{id}`

### Bitacora

Controller: `TicketBitacoraController`

- `GET /api/Ticket/{idTicket}/bitacora`
- `POST /api/Ticket/{idTicket}/bitacora`
- `GET /api/TicketBitacora/{id}`
- `PUT /api/TicketBitacora/{id}`
- `DELETE /api/TicketBitacora/{id}`

### SLA

Controllers:

- `SlaPoliticaController`
- `SlaReglaController`
- `TicketSlaController`

Gestionan politicas SLA, reglas por prioridad/categoria y seguimiento de SLA por ticket.

### Notificaciones

Controller: `NotificacionController`

Endpoints para frontend:

- `GET /api/Notificacion`
  - Lista paginada y filtrable.
- `GET /api/Notificacion/mis-notificaciones`
  - Notificaciones del usuario autenticado.
- `GET /api/Notificacion/no-leidas/count`
  - Contador para badge/campana.
- `GET /api/Notificacion/{id}`
- `POST /api/Notificacion`
- `PUT /api/Notificacion/{id}/marcar-leida`
- `PUT /api/Notificacion/{id}/marcar-no-leida`
- `PUT /api/Notificacion/marcar-leidas`
- `PUT /api/Notificacion/marcar-todas-leidas`
- `DELETE /api/Notificacion/{id}`

Las notificaciones automaticas de tickets se crean para el usuario asignado al ticket y no para el usuario que ejecuta el cambio.

### Graficos

Controller: `GraficosController`

- `GET /api/Graficos/resumen`
- `GET /api/Graficos/tickets-por-estado`
- `GET /api/Graficos/tickets-por-prioridad`
- `GET /api/Graficos/tickets-por-categoria`
- `GET /api/Graficos/tickets-creados-por-mes`
- `GET /api/Graficos/sla-cumplimiento`
- `GET /api/Graficos/tickets-por-responsable`

Todos respetan permisos por rol y visibilidad de tickets.

## Mantenedores

Catalogos base:

- `EstadoTicketController`
- `PrioridadTicketController`
- `CategoriaTicketController`
- `SubcategoriaTicketController`
- `EquipoSoporteController`
- `TipoRelacionTicketController`

Configuracion operacional:

- `CategoriaResponsableController`
- `EquipoSoporteUsuarioController`
- `CategoriaEquipoSoporteController`
- `FlujoEstadoTicketController`

Varios mantenedores incluyen endpoints `select` para alimentar formularios del frontend.

## Servicios internos

- `CurrentUserService`
  - Centraliza usuario autenticado y roles.
- `ReferenceValidationService`
  - Valida existencia y referencias activas.
- `TicketAccessService`
  - Aplica visibilidad por rol y configuracion.
- `TicketAttachmentService`
  - Valida y guarda archivos.
- `TicketHistoryService`
  - Construye historial y mensajes descriptivos.
- `TicketNotificationService`
  - Crea notificaciones automaticas de eventos relevantes del ticket.

## Reglas importantes de negocio

- El solicitante de un ticket es siempre el usuario autenticado.
- El responsable inicial sale de `CategoriaResponsable` segun la categoria de la subcategoria.
- Los cambios relevantes del ticket dejan historial.
- El flujo de estados se valida contra `FlujoEstadoTicket`.
- El resolvedor solo ve tickets asignados o permitidos por su configuracion de equipo/categoria.
- El administrador ve todo.
- El registro de usuarios crea cuentas con rol `Solicitante` por defecto.
- Solo usuarios activos pueden autenticarse y ser usados como referencias en asignaciones/configuraciones.
- La creacion de tickets no recibe estado desde el cliente; el backend define el estado inicial y registra la transicion automatica correspondiente.
- Un ticket debe tener prioridad y subcategoria activas para ser creado o actualizado.
- Una subcategoria solo es valida si ella y su categoria estan activas.
- Para crear un ticket, la categoria asociada debe tener un responsable activo configurado.
- La asignacion de un ticket toma el usuario modificador desde el usuario autenticado, no desde el request.
- El cambio de estado debe existir como flujo activo entre el estado actual y el estado destino.
- Si el flujo de estado requiere comentario, el cambio no se permite sin comentario.
- Cuando un ticket llega a un estado final, se registran fechas de resolucion/cierre si aun no existen.
- Los cambios de prioridad validan que la prioridad destino exista y este activa.
- La regla SLA se busca primero por prioridad y categoria; si no existe, se usa una regla global por prioridad.
- Cada ticket debe tener como maximo un SLA activo.
- Las reglas SLA no deben duplicarse por politica, prioridad y categoria.
- Los mantenedores usan borrado logico mediante `Activo` cuando la entidad lo soporta.
- Los endpoints `select` devuelven datos livianos para formularios del frontend.
- Los adjuntos se guardan en disco y en base de datos; si falla la transaccion, se eliminan los archivos guardados.
- La descarga de adjuntos valida que la ruta fisica este dentro de la carpeta `Uploads`.
- Las bitacoras pueden ser internas o visibles segun el campo `EsInterno`.
- Las notificaciones automaticas de tickets se crean para el usuario asignado y no para el usuario que ejecuta el cambio.
- Los solicitantes solo pueden ver informacion de sus propios tickets.
- Los resolvedores ven tickets asignados y tickets permitidos por la relacion categoria/equipo soporte.
- El administrador puede consultar y operar sobre toda la informacion protegida.
- Los listados principales usan paginacion para evitar respuestas grandes.
- Los filtros por fechas validan que la fecha desde no sea mayor que la fecha hasta.
