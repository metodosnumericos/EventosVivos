# EventosVivos — Reservation Core

Sistema de reservas para EventosVivos implementado como proyecto de evaluación de ingeniería de software.

## Propósito

Resolver tres problemas operativos críticos de EventosVivos:
1. **Sobreventa**: falta de control de capacidad en tiempo real con concurrencia.
2. **Conflictos de venue**: dos eventos activos no pueden compartir venue y rango de tiempo.
3. **Validación manual**: reglas de negocio dispersas en UI sin respaldo de backend.

---

## Arquitectura

El sistema usa **Modular Monolith con Clean Architecture** en el backend:

```
EventosVivos.Domain          Entidades, invariantes, reglas de capacidad, transiciones de estado
EventosVivos.Application     Casos de uso, contratos de repositorio
EventosVivos.Infrastructure  EF Core, PostgreSQL, seed data, proveedor de tiempo
EventosVivos.Api             Endpoints REST, autenticación, middleware, CORS
```

**Por qué modular monolith y no microservicios**: el problema es un único bounded context (reservas de eventos). Microservicios agregarían complejidad distribuida sin beneficio justificable dentro del tiempo de entrega y el alcance del MVP.

**Por qué Clean Architecture**: la evaluación pondera arquitectura, modularidad y testabilidad. Las reglas de negocio deben poder probarse sin arrancar Angular, sin conectarse a PostgreSQL, y sin levantar el API — esto sólo es posible si el Domain y Application son proyectos independientes sin dependencias de infraestructura.

---

## Tecnologías

| Componente | Tecnología |
|---|---|
| Backend | .NET 10 LTS, ASP.NET Core Minimal APIs |
| ORM / BD | Entity Framework Core 10, PostgreSQL 17 |
| Tests | xUnit, NSubstitute, Testcontainers.PostgreSql, WebApplicationFactory |
| Frontend | Angular 22, standalone components, Reactive Forms |
| Contenedor de BD local | Docker Compose + postgres:17-alpine |

---

## Ejecución local

### Prerrequisitos

- .NET 10 SDK
- Node.js 20+
- Angular CLI 22 (`npm install -g @angular/cli`)
- Docker Desktop (para PostgreSQL via Docker Compose)

### 1. Levantar la base de datos

```bash
docker compose up -d db
```

O configure la variable de entorno con un PostgreSQL local:

```bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=eventosvivos;Username=eventosvivos;Password=eventosvivos"
```

### 2. Arrancar el backend

```bash
dotnet restore
dotnet run --project src/EventosVivos.Api
```

El API levanta en `http://localhost:5000` y aplica las migraciones y seed de venues automáticamente.

### 3. Arrancar el frontend

```bash
cd src/eventosvivos-web
npm install
ng serve
```

Frontend disponible en `http://localhost:4200`.

---

## Tests

```bash
dotnet test
```

Incluye:
- Tests unitarios de dominio (`EventosVivos.Domain.Tests`): reglas de negocio, transiciones de estado, restricciones nocturnas de fin de semana.
- Tests unitarios de aplicación (`EventosVivos.Application.Tests`): casos de uso con mocks NSubstitute — validación de límites de cantidad, capacidad, confirmación doble.
- Tests de integración de API (`EventosVivos.Api.Tests`): WebApplicationFactory + Testcontainers PostgreSQL real, incluyendo el **test de race condition de concurrencia**.

---

## Reglas de negocio

| ID | Regla |
|----|-------|
| RN01 | Capacidad del evento ≤ capacidad del venue. |
| RN02 | Eventos activos no pueden superponerse en el mismo venue. |
| RN03 | Eventos de fin de semana no pueden iniciar después de las 22:00 (America/Bogota). |
| RN04 | Reservas bloqueadas para eventos ya iniciados, completados, cancelados, o a menos de 1 hora de inicio. |
| RF03 | Eventos a menos de 24 horas permiten máximo 5 entradas por transacción. |
| RN05 | Eventos con precio mayor a 100 permiten máximo 10 entradas por transacción. |
| RN06 | El estado "completado" es derivado: `now (Bogotá) > endsAt`. |
| RN07 | Cancelaciones confirmadas a menos de 48 horas del evento generan capacidad perdida (no disponible para reventa). |

### Precedencia de reglas de cantidad

Los bloques duros se aplican primero: evento cancelado, completado, ya iniciado, o a menos de 1 hora. Si procede la reserva, se calculan todos los límites aplicables y gana el más estricto. Ejemplo: un evento a 12 horas con precio > 100 permite como máximo 5 entradas (el de 24h gana al de precio).

---

## Estrategia de concurrencia

El agregado `Event` tiene un token de concurrencia optimista: el entero `Version`, manejado manualmente e incrementado por todos los comandos que modifican capacidad disponible (creación de reserva, cancelación pendiente, cancelación confirmada que libera).

El `UnitOfWork` captura `DbUpdateConcurrencyException` y lanza `OptimisticConcurrencyException`. El caso de uso `CreateReservationUseCase` reintenta hasta 3 veces con capacidad fresca antes de devolver un 409.

El test `ConcurrentReservationTests.ConcurrentReservations_NeverOversell` lanza 10 tareas paralelas compitiendo por el último cupo de un evento con capacidad 1. Exactamente 1 reserva debe tener éxito y las demás deben recibir 409. Este test corre contra PostgreSQL real via Testcontainers.

---

## Política de zona horaria

Los timestamps se almacenan como instantes UTC. Todas las reglas de negocio que involucran calendario local (restricción de fin de semana, corte de 22:00, ventanas de 1h/24h/48h, estado completado) se evalúan convirtiendo el instante a `America/Bogota` antes de comparar. La implementación concreta es `BogotaTimeProvider` inyectable, lo que permite usar tiempos simulados en tests.

---

## Seguridad

- **Admin API Key**: Los endpoints de creación de eventos, listado de reservas, confirmación de pago y cancelación administrativa requieren el header `X-Admin-Key`. En desarrollo el valor es `dev-admin-key` (configurable via `appsettings.json` o variable de entorno `AdminApiKey`). La clave **no se incluye en el bundle de Angular**: el campo de entrada en el header de la SPA escribe el valor en `sessionStorage` (via `AdminAuthService`), y un `HttpInterceptorFn` lo inyecta en todas las peticiones a rutas admin en tiempo de ejecución. La clave desaparece al cerrar la pestaña.
- **Buyer cancellation**: Los compradores pueden cancelar usando `id + buyerEmail` (reserva pendiente) o `id + buyerEmail + reservationCode` (reserva confirmada) — sin credenciales de admin.
- **CORS**: Restringido al origin de Angular configurado en `AllowedOrigin`.
- **ProblemDetails**: Los errores devuelven RFC 9457 `ProblemDetails` sin stack traces ni detalles internos.
- **DTOs sobre entidades**: Todos los endpoints usan request DTOs (`CreateEventRequest`, `CreateReservationRequest`, etc.) para prevenir over-posting.
- **HTTPS**: Se recomienda HTTPS en deployment. El certificado de desarrollo puede confiarse con `dotnet dev-certs https --trust`.

### Semántica HTTP de errores

- `409 Conflict`: conflicto con el estado actual del recurso — capacidad agotada, solapamiento de venue, código duplicado, token de concurrencia desactualizado, transición de estado inválida.
- `422 Unprocessable Entity`: rechazo de regla de negocio semántica — ventana de reserva cerrada, límite de cantidad por transacción, restricción de horario de fin de semana.
- `404 Not Found`: recurso no encontrado.
- `400 Bad Request`: error de validación de input.
- `401 Unauthorized`: credencial de admin ausente o inválida.

---

## Capacidad pendiente y mejoras de producción

Las reservas pendientes retienen capacidad en el MVP para prevenir sobreventa. Un sistema de producción agregaría:

- **Expiración automática**: timestamp de expiración en la reserva + job de liberación periódica.
- **HTTPS obligatorio**: redirección HTTP → HTTPS en deployment.
- **Autenticación completa**: reemplazar la API Key de demo con JWT/Bearer Token — gestión de roles (admin/comprador) con registro de usuarios y tokens de corta duración.
- **Rate limiting**: protección contra abuso de la API pública de reservas.

---

## Decisiones de arquitectura

### 1. Restricción de exclusión PostgreSQL para solapamiento de venue

**Problema**: `HasActiveOverlapAsync` + `INSERT` no son atómicos — con dos transacciones concurrentes ambas pasan la consulta y luego ambas insertan, creando un solapamiento de venue silencioso.

**Alternativas consideradas**:
- Transacciones serializables: requieren reintentos en toda la capa de aplicación y degradan throughput.
- `SELECT FOR UPDATE` con lock por venue: bloqueo pesimista que serializa todas las creaciones de eventos, no solo las del mismo venue.
- Advisory locks de PostgreSQL: lock explícito por venue_id, pero requiere gestión manual y no escala bien.

**Decisión**: restricción de exclusión `EXCLUDE USING gist` sobre `(venue_id, tstzrange(starts_at, ends_at)) WHERE (state = 'Active')`. PostgreSQL garantiza la atomicidad a nivel de constraint, sin necesidad de serializar transacciones ajenas. El `UnitOfWork` captura `SqlState 23P01` y lo traduce a `VenueConflictException` → 409.

---

### 2. UnitOfWork como traductor de excepciones de infraestructura

**Decisión**: `UnitOfWork.SaveChangesAsync` captura `DbUpdateException` y las convierte en excepciones de dominio antes de propagarlas (`VenueConflictException`, `DuplicateReservationCodeException`, `OptimisticConcurrencyException`). Los casos de uso sólo ven excepciones de dominio; nunca manejan `PostgresException` ni `DbUpdateException`.

**Por qué**: mantiene los casos de uso limpios de conocimiento de la capa de persistencia. Si se cambia el proveedor de base de datos, sólo cambia el `UnitOfWork`; la lógica de aplicación no toca.

---

### 3. Concurrencia optimista con token de versión explícito

**Decisión**: `Event.Version` es un entero que el dominio incrementa manualmente (no via `[ConcurrencyToken]` de EF). `UnitOfWork` lo expone como token de concurrencia en la configuración de EF, de forma que una escritura concurrente genera `DbUpdateConcurrencyException`. `CreateReservationUseCase` reintenta hasta 3 veces recargando el agregado fresco.

**Detalle de implementación**: después de un fallo optimista, el agregado anterior debe desconectarse del identity map de EF antes de recargar. Si no, `GetByIdAsync` devuelve la entidad stale del tracker en lugar de la fila actual de la BD. Por eso `IUnitOfWork` expone `Detach<T>` — para que el caso de uso pueda limpiar el tracker sin acceder directamente al `DbContext`.

---

### 4. Estado derivado vs. estado almacenado para "Completado"

**Decisión**: `Event.GetEffectiveState()` computa el estado derivado en memoria en lugar de persistir una columna `state` que deba actualizarse periódicamente.

**Por qué**: no requiere job de mantenimiento, no hay inconsistencias de sincronización, y el dominio es la única fuente de verdad. El trade-off (no se puede filtrar por `state = 'Completed'` en SQL de forma directa) es aceptable en el MVP. Una vista materializada o columna computada resuelta en PostgreSQL sería la evolución natural si los filtros por estado completado se vuelven frecuentes.

---

### 5. Código de reserva único con reintentos a nivel de caso de uso

**Decisión**: `ConfirmPaymentUseCase` genera `EV-XXXXXX`, verifica contra `CodeExistsAsync` (pre-check), y si el `UnitOfWork` lanza `DuplicateReservationCodeException` (SqlState 23505 en la constraint unique de `reservation_code`), desconecta la entidad stale, recarga el estado fresco y reintenta con un código nuevo.

**Por qué dos capas de protección**: el pre-check con `CodeExistsAsync` evita roundtrips innecesarios bajo carga normal. La captura del error de BD cubre la race condition residual entre el pre-check y el INSERT, sin necesidad de un lock.

---

## Endpoints

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/venues` | - | Listar venues |
| GET | `/api/events` | - | Listar eventos (filtros opcionales) |
| POST | `/api/events` | Admin | Crear evento |
| GET | `/api/events/{id}/occupancy` | - | Reporte de ocupación |
| GET | `/api/reservations` | Admin | Listar reservas (filtro opcional `?eventId=X`) |
| POST | `/api/reservations` | - | Crear reserva |
| POST | `/api/reservations/{id}/confirm` | Admin | Confirmar pago |
| POST | `/api/reservations/{id}/cancel` | Admin | Cancelar reserva (admin) |
| POST | `/api/reservations/{id}/buyer-cancel` | - | Cancelar reserva (comprador) |

Documentación OpenAPI disponible en `/openapi/v1.json` (desarrollo).
