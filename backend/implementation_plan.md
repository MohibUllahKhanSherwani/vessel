# Vessel Backend Implementation Plan

## Working Setup

- Target framework: `net10.0`
- Backend style for v1: ASP.NET Core 10 Web API with controllers
- Database: PostgreSQL
- Vector store: `pgvector`
- Cache: Redis
- Real-time: SignalR
- Background jobs: Hangfire
- Tests: xUnit

## Working Rules

- Use controllers for v1. Do not mix minimal APIs into the same backend while the core flows are still being built.
- Keep domain entities in `Vessel.Core`.
- Keep DTOs, validators, service interfaces, and persistence interfaces in `Vessel.Application`.
- Keep EF Core, Redis, Hangfire, and external integrations in `Vessel.Infrastructure`.
- Keep use-case service implementations in `Vessel.Application` and infrastructure implementations in `Vessel.Infrastructure`.
- `Vessel.API` is the composition root. Controllers should not use `DbContext` directly.
- Do not add a generic repository just to match a pattern name. If a use case needs data-access abstractions, add small feature-specific repository or query interfaces only.
- Public registration creates `Consumer` users only.
- Use UTC for all timestamps.
- Keep all I/O async.

### Routing And Constraints

- Use **attribute routing** on every controller. Do not rely on conventional routing.
- Decorate each controller class with `[Route("api/[controller]")]` and `[ApiController]`.
- Decorate each action with explicit HTTP-method attributes: `[HttpGet]`, `[HttpPost]`, `[HttpPatch]`, `[HttpDelete]`.
- Apply **route constraints** on path parameters wherever the type is known. Examples:
  - `[HttpGet("{id:guid}")]`
  - `[HttpGet("{areaId:guid}/{providerId:guid}")]`
- Validate **query parameters** using FluentValidation on the request DTO. Bind simple scalar parameters directly with `[FromQuery]` and provide defaults where applicable, e.g. `[FromQuery] int days = 30`.
- Never expose an endpoint without an explicit route template.

### Swagger / OpenAPI

- Swagger UI must be available from Phase 1 onward. It is the primary way to demo and test the API before a frontend exists.
- Use `Swashbuckle.AspNetCore` for OpenAPI document generation.
- Add XML doc comments (`<summary>`, `<param>`, `<response>`) to every controller action. Enable XML comment generation in the `.csproj`.
- Tag controllers with `[Tags("...")]` so Swagger groups endpoints logically.
- Configure Swagger to include the JWT bearer scheme so tokens can be tested directly from the Swagger UI.
- Add `[ProducesResponseType]` attributes on every action to document expected status codes.

### Dependency Injection Strategy (Mock-First)

- Every service and repository is consumed through its interface. Concrete classes are never injected directly.
- In early phases, register **mock / stub implementations** for services that do not exist yet. This lets controllers compile, Swagger shows the full API surface, and integration tests can run against predictable data.
- As each phase is completed, **swap the mock registration for the real implementation** in `ServiceCollectionExtensions`. The controller code does not change.
- Progression pattern per phase:
  1. Define the interface in `Vessel.Application`.
  2. Create a `Mock___Service` in `Vessel.Infrastructure/Mocks` that returns hard-coded in-memory data.
  3. Register the mock in DI.
  4. Build and test the controller against the mock.
  5. Implement the real service.
  6. Replace the mock registration with the real one.
  7. Move the mock class to `Vessel.Tests/Mocks/` for integration test use. Delete it from `Vessel.Infrastructure/Mocks/`.
- Store mock implementations in `Vessel.Infrastructure/Mocks/`.

## Solution Layout

| Project | Type | Purpose |
|---|---|---|
| `Vessel.Core` | Class library | Entities, enums, domain exceptions |
| `Vessel.Application` | Class library | DTOs, validators, service interfaces, persistence interfaces, business rules |
| `Vessel.Infrastructure` | Class library | EF Core, repository/query implementations, Redis, auth persistence, jobs |
| `Vessel.API` | ASP.NET Core Web API | Controllers, middleware, DI, auth, SignalR, Swagger |
| `Vessel.AI` | Class library | Embeddings, RAG orchestration, prompt handling |
| `Vessel.Tests` | xUnit | Unit tests and integration tests |

## Phase 1: Bootstrap And Local Setup

### Tasks

1. Create the solution and projects from `backend/`.

```powershell
dotnet new sln -n Vessel

dotnet new classlib -n Vessel.Core -o Vessel.Core -f net10.0
dotnet new classlib -n Vessel.Application -o Vessel.Application -f net10.0
dotnet new classlib -n Vessel.Infrastructure -o Vessel.Infrastructure -f net10.0
dotnet new classlib -n Vessel.AI -o Vessel.AI -f net10.0
dotnet new webapi -n Vessel.API -o Vessel.API -f net10.0 --use-controllers
dotnet new xunit -n Vessel.Tests -o Vessel.Tests -f net10.0
```

2. Add all projects to the solution.

```powershell
dotnet sln Vessel.sln add Vessel.Core/Vessel.Core.csproj
dotnet sln Vessel.sln add Vessel.Application/Vessel.Application.csproj
dotnet sln Vessel.sln add Vessel.Infrastructure/Vessel.Infrastructure.csproj
dotnet sln Vessel.sln add Vessel.AI/Vessel.AI.csproj
dotnet sln Vessel.sln add Vessel.API/Vessel.API.csproj
dotnet sln Vessel.sln add Vessel.Tests/Vessel.Tests.csproj
```

3. Add project references.

```powershell
dotnet add Vessel.Application/Vessel.Application.csproj reference Vessel.Core/Vessel.Core.csproj

dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj reference Vessel.Core/Vessel.Core.csproj
dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj reference Vessel.Application/Vessel.Application.csproj

dotnet add Vessel.AI/Vessel.AI.csproj reference Vessel.Core/Vessel.Core.csproj
dotnet add Vessel.AI/Vessel.AI.csproj reference Vessel.Application/Vessel.Application.csproj

dotnet add Vessel.API/Vessel.API.csproj reference Vessel.Application/Vessel.Application.csproj
dotnet add Vessel.API/Vessel.API.csproj reference Vessel.Infrastructure/Vessel.Infrastructure.csproj
dotnet add Vessel.API/Vessel.API.csproj reference Vessel.AI/Vessel.AI.csproj

dotnet add Vessel.Tests/Vessel.Tests.csproj reference Vessel.Core/Vessel.Core.csproj
dotnet add Vessel.Tests/Vessel.Tests.csproj reference Vessel.Application/Vessel.Application.csproj
dotnet add Vessel.Tests/Vessel.Tests.csproj reference Vessel.Infrastructure/Vessel.Infrastructure.csproj
dotnet add Vessel.Tests/Vessel.Tests.csproj reference Vessel.API/Vessel.API.csproj
```

4. Install the first packages.

```powershell
dotnet add Vessel.Application/Vessel.Application.csproj package FluentValidation

dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL

dotnet add Vessel.API/Vessel.API.csproj package Swashbuckle.AspNetCore

dotnet add Vessel.Tests/Vessel.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add Vessel.Tests/Vessel.Tests.csproj package FluentAssertions
```

5. Install the EF Core CLI tool.

```powershell
dotnet tool update --global dotnet-ef
```

This command installs the tool if missing and updates it if already installed.

6. Create `backend/docker-compose.yml`.

```yaml
services:
  postgres:
    image: pgvector/pgvector:pg16
    container_name: vessel-postgres
    environment:
      POSTGRES_DB: vessel
      POSTGRES_USER: vessel
      POSTGRES_PASSWORD: vessel
    ports:
      - "5432:5432"
    volumes:
      - vessel-postgres-data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    container_name: vessel-redis
    ports:
      - "6379:6379"

volumes:
  vessel-postgres-data:
```

7. Start local services.

```powershell
docker compose up -d
```

8. Create `Vessel.API/appsettings.Development.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=vessel;Username=vessel;Password=vessel",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Issuer": "Vessel",
    "Audience": "Vessel.Client",
    "Key": "replace-this-with-a-long-development-secret-key"
  }
}
```

9. Delete template files from `Vessel.API`.

- `WeatherForecast.cs`
- `WeatherForecastController.cs`

10. Clean `Program.cs` so only your own registrations remain.

11. Configure Swagger in `Program.cs`.

- Register `AddSwaggerGen()` with API info (title: "Vessel API", version: "v1").
- Add a JWT security definition so the Swagger UI has an "Authorize" button.
- Enable `UseSwagger()` and `UseSwaggerUI()` in the pipeline.
- Enable XML comments: add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to `Vessel.API.csproj` and pass the XML path to `SwaggerGenOptions.IncludeXmlComments()`.

12. Create `Vessel.API/Extensions/ServiceCollectionExtensions.cs`.

- This file will hold all DI registrations grouped by feature area.
- Start with a method `AddApplicationServices(this IServiceCollection services)` that registers nothing yet. Each phase will add to it.
- Call it from `Program.cs`.

13. Create a `HealthController` as a smoke-test endpoint.

- `[Route("api/[controller]")]`, `[ApiController]`
- `[HttpGet]` returns `200 OK` with `{ "status": "healthy" }`.
- Add `[ProducesResponseType(typeof(object), 200)]`.
- This confirms attribute routing, Swagger, and the pipeline work.

14. Build the solution.

```powershell
dotnet build
```

15. Run the API once to confirm the solution starts.

```powershell
dotnet run --project Vessel.API
```

16. Open Swagger UI in a browser at `https://localhost:{port}/swagger` to verify it loads.

### Done When

- `dotnet build` succeeds
- PostgreSQL and Redis are running
- `Vessel.API` starts without template errors
- Swagger UI loads and shows the health endpoint
- `ServiceCollectionExtensions.cs` exists and is called from `Program.cs`

## Phase 2: Domain Model And Persistence

### Tasks

1. Create these folders in `Vessel.Core`.

- `Entities/`
- `Enums/`
- `Exceptions/`

2. Create these enums.

- `UserRole`: `Consumer`, `Provider`, `Admin`
- `BookingStatus`: `Pending`, `Confirmed`, `Cancelled`
- `AlertDirection`: `AboveOrEqual`, `BelowOrEqual`

3. Create these entities in `Vessel.Core/Entities`.

- `User`
  - `Id`
  - `Email`
  - `PasswordHash`
  - `FullName`
  - `Role`
  - `IsActive`
  - `CreatedAt`
  - `UpdatedAt`

- `RefreshToken`
  - `Id`
  - `UserId`
  - `TokenHash`
  - `ExpiresAt`
  - `CreatedAt`
  - `RevokedAt`
  - `ReplacedByTokenHash`

- `Area`
  - `Id`
  - `City`
  - `Name`
  - `Latitude`
  - `Longitude`

- `Provider`
  - `Id`
  - `UserId`
  - `CompanyName`
  - `ContactNumber`
  - `IsActive`
  - `CreatedAt`

- `ProviderRate`
  - `Id`
  - `ProviderId`
  - `AreaId`
  - `PricePerGallon`
  - `EffectiveFrom`
  - `EffectiveTo`
  - `CreatedAt`

- `Booking`
  - `Id`
  - `ConsumerId`
  - `ProviderId`
  - `AreaId`
  - `IdempotencyKey`
  - `VolumeInGallons`
  - `PricePerGallonSnapshot`
  - `TotalPrice`
  - `DeliveryAddress`
  - `Notes`
  - `Status`
  - `ScheduledFor`
  - `CreatedAt`
  - `UpdatedAt`

- `PriceAlert`
  - `Id`
  - `ConsumerId`
  - `AreaId`
  - `ThresholdPrice`
  - `TargetVolumeInGallons`
  - `Direction`
  - `IsActive`
  - `LastTriggeredRateId`
  - `CreatedAt`

4. Do not create `RateEmbedding` yet. Add it in phase 9 with the AI migration.

5. Create `ApplicationDbContext` in `Vessel.Infrastructure/Data`.

6. Create this folder in `Vessel.Application`.

- `Interfaces/Repositories/`

7. Add only the repository interfaces you need for the first backend features.

Start with:

- `IUserRepository`
- `IRefreshTokenRepository`
- `IAreaRepository`
- `IProviderRepository`
- `IProviderRateRepository`
- `IBookingRepository`
- `IPriceAlertRepository`

8. Implement those interfaces in `Vessel.Infrastructure`.

9. Create one EF Core configuration class per entity in `Vessel.Infrastructure/Data/Configurations`.

10. Add database constraints and indexes.

- Unique index on `Users.Email`
- Unique index on `Areas.City + Areas.Name`
- Unique index on `Providers.UserId`
- Unique index on `Bookings.ConsumerId + IdempotencyKey`
- Filtered unique index on `ProviderRates.ProviderId + AreaId` where `EffectiveTo IS NULL`

11. Set decimal precision explicitly for price fields.

- `PricePerGallon`
- `PricePerGallonSnapshot`
- `TotalPrice`
- `ThresholdPrice`

12. Configure all timestamps in UTC.

13. Seed a starter list of areas for Islamabad, Lahore, and Karachi in `ApplicationDbContext.OnModelCreating` using `HasData`. Do not build admin area management in v1.

14. Create the initial migration.

```powershell
dotnet ef migrations add InitialCreate --project Vessel.Infrastructure --startup-project Vessel.API --output-dir Data/Migrations
```

15. Apply the migration.

```powershell
dotnet ef database update --project Vessel.Infrastructure --startup-project Vessel.API
```

Note:

- `--project` is the project that contains the `DbContext`
- `--startup-project` is the executable project that loads configuration

### Done When

- The schema exists in PostgreSQL
- All entities map correctly
- The area seed data is inserted
- The unique constraints are enforced by the database

## Phase 3: Authentication And Authorization

### Tasks

1. Install the auth package in `Vessel.API`.

```powershell
dotnet add Vessel.API/Vessel.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
```

2. Create these folders in `Vessel.Application`.

- `DTOs/Auth/`
- `Interfaces/`
- `Services/`

3. Create auth DTOs.

- `RegisterRequestDto`
- `LoginRequestDto`
- `RefreshTokenRequestDto`
- `AuthResponseDto`

4. Create service interfaces.

- `IAuthService`
- `IJwtTokenService`
- `IPasswordHasherService`

5. Implement auth services.

- implement `AuthService` in `Vessel.Application/Services`
- implement `JwtTokenService` in `Vessel.Infrastructure/Auth`
- implement `PasswordHasherService` in `Vessel.Infrastructure/Auth`
- hash passwords securely
- create access tokens
- create refresh tokens
- store only refresh token hashes in the database
- revoke old refresh tokens on refresh

6. Configure JWT authentication in `Program.cs`.

- Read `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:Key` from configuration.
- Register `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` with `AddJwtBearer()`.
- Validate `Issuer`, `Audience`, `IssuerSigningKey`, and `Lifetime`.
- Set `RequireHttpsMetadata = false` for local development only.
- Place `UseAuthentication()` before `UseAuthorization()` in the pipeline.

7. Add authorization policies.

- `ConsumerOnly` — requires `Role` claim == `Consumer`
- `ProviderOnly` — requires `Role` claim == `Provider`
- `AdminOnly` — requires `Role` claim == `Admin`
- Register them with `AddAuthorizationBuilder().AddPolicy(...)` in `Program.cs`.

8. Update Swagger configuration to support JWT.

- The security definition from Phase 1 should already be in place.
- Test the full flow: register → login → copy the token → click "Authorize" in Swagger UI → paste `Bearer {token}` → hit a protected endpoint.

9. Create `AuthController` with attribute routing and response annotations.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Authentication")]`
- `[HttpPost("register")]` — `[ProducesResponseType(typeof(AuthResponseDto), 201)]`, `[ProducesResponseType(400)]`, `[ProducesResponseType(409)]`
- `[HttpPost("login")]` — `[ProducesResponseType(typeof(AuthResponseDto), 200)]`, `[ProducesResponseType(401)]`
- `[HttpPost("refresh")]` — `[ProducesResponseType(typeof(AuthResponseDto), 200)]`, `[ProducesResponseType(401)]`
- Add XML `<summary>` comments to each action.

10. Register auth services in `ServiceCollectionExtensions`.

- `IAuthService` → `AuthService`
- `IJwtTokenService` → `JwtTokenService`
- `IPasswordHasherService` → `PasswordHasherService`

11. Make public registration create `Consumer` users only.

12. Seed one admin account and one provider account for testing.

13. Add validators for the auth request DTOs.

14. Add tests for:

- successful registration
- successful login
- failed login
- refresh token rotation
- provider access blocked for consumer users

### Done When

- A consumer can register and log in
- Access and refresh tokens are issued correctly
- Refresh token rotation works
- Role-based authorization is enforced
- JWT "Authorize" button works in Swagger UI
- All auth endpoints show documented request/response schemas in Swagger

## Phase 4: Areas And Rate Intelligence

### Mock-First Approach

Before building the real `IRateService`, create `MockRateService` in `Vessel.Infrastructure/Mocks/` that returns a few hard-coded rates. Register it in DI. This lets you build and visually test the `RatesController` and Swagger docs before writing any EF Core queries. Replace the mock with the real service once the repository logic is done.

### Tasks

1. Create these folders if they do not exist yet.

- `Vessel.Application/DTOs/Areas/`
- `Vessel.Application/DTOs/Rates/`
- `Vessel.API/Controllers/`
- `Vessel.API/Hubs/`

2. Create DTOs.

- `AreaDto`
- `RateDto`
- `CreateRateDto`
- `RateHistoryDto`

3. Create service interfaces.

- `IAreaService`
- `IRateService`

4. Create mock implementations and register them in DI.

- `MockAreaService` — returns the seeded area list from memory.
- `MockRateService` — returns a few fake rates.

5. Create controllers with full attribute routing.

- `AreasController` — `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Areas")]`
- `RatesController` — `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Rates")]`

6. Implement these endpoints with route constraints.

- `[HttpGet]` on `AreasController` → `GET /api/areas`
- `[HttpGet("{areaId:guid}")]` on `RatesController` → `GET /api/rates/{areaId}`
- `[HttpGet("history/{areaId:guid}/{providerId:guid}")]` on `RatesController` → `GET /api/rates/history/{areaId}/{providerId}`
- `[HttpPost]` on `RatesController` → `POST /api/rates`

7. Add response annotations to every action.

- `AreasController.GetAll`: `[ProducesResponseType(typeof(List<AreaDto>), 200)]`
- `RatesController.GetByArea`: `[ProducesResponseType(typeof(List<RateDto>), 200)]`, `[ProducesResponseType(404)]`
- `RatesController.GetHistory`: `[ProducesResponseType(typeof(List<RateHistoryDto>), 200)]`, `[ProducesResponseType(404)]`
- `RatesController.Create`: `[ProducesResponseType(typeof(RateDto), 201)]`, `[ProducesResponseType(400)]`, `[ProducesResponseType(401)]`, `[ProducesResponseType(403)]`

8. Verify endpoints render correctly in Swagger UI while still backed by mocks.

9. Implement real `AreaService` and `RateService`. Swap mock registrations for real ones in `ServiceCollectionExtensions`.

10. Lock `POST /api/rates` to provider users only using `[Authorize(Policy = "ProviderOnly")]`.

11. In the rate update flow:

- find the current active rate for that provider and area
- set its `EffectiveTo` to the current UTC time
- insert a new row with the new `PricePerGallon`
- set `EffectiveFrom` to the current UTC time
- save both changes in one transaction

12. Add `RateAlertHub` in `Vessel.API/Hubs`.

13. After a rate update succeeds, broadcast a `RateChanged` SignalR event.

14. Add tests for:

- posting a new rate expires the old active rate
- history returns older rates and latest rates correctly
- public rate queries return only active rates

### Done When

- Providers can create new rates
- Rate history is preserved
- Only one active rate exists per provider-area pair
- SignalR broadcasts on successful rate changes
- Mock services are removed or moved to the test project
- Swagger shows all rate endpoints with full schemas

## Phase 5: Provider Discovery

### Mock-First Approach

Create `MockProviderDiscoveryService` that returns a handful of static providers with pre-calculated distances. This lets you finalize the controller, query parameters, and Swagger docs before implementing Haversine.

### Tasks

1. Create DTOs for provider search.

- `ProviderSearchResultDto`

2. Create service interface.

- `IProviderDiscoveryService`

3. Create `MockProviderDiscoveryService` in `Vessel.Infrastructure/Mocks/`. Register it in DI.

4. Create `ProvidersController`.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Providers")]`

5. Implement this endpoint with query-parameter binding.

- `[HttpGet("search")]` → `GET /api/providers/search?lat={lat}&lon={lon}&radiusKm={radiusKm}`
- Bind parameters as `[FromQuery] double lat`, `[FromQuery] double lon`, `[FromQuery] double radiusKm = 10`.
- Add `[ProducesResponseType(typeof(List<ProviderSearchResultDto>), 200)]`.
- Add `[ProducesResponseType(400)]` for invalid coordinates.

6. Add a FluentValidation validator for query parameters.

- `lat` must be between -90 and 90.
- `lon` must be between -180 and 180.
- `radiusKm` must be > 0 and <= 100 (sensible upper bound for v1).

7. Verify the endpoint works in Swagger with the mock.

8. Implement real `ProviderDiscoveryService`. Swap the mock registration.

9. Implement the Haversine formula yourself in C#.

10. Use the area coordinates as the distance source.

11. Query nearby areas, join to active provider rates, and return providers that serve those areas.

12. Sort results by:

- distance first
- price second

13. Return these fields in the response DTO.

- provider id
- company name
- area id
- area name
- city
- distance in kilometers
- current price per gallon

14. Add tests for:

- providers outside the radius are excluded
- sorting is distance first and price second
- invalid coordinates return 400

### Done When

- Geo search returns the correct providers
- Sorting matches the PRD
- Query parameters are validated
- Swagger shows the search endpoint with all query parameters documented

## Phase 6: Booking Engine And Idempotency

### Mock-First Approach

Create `MockBookingService` that stores bookings in a `ConcurrentDictionary` and implements idempotency checks in-memory. This lets you build the controller, validate the `Idempotency-Key` header contract, and verify Swagger docs before wiring Redis and EF Core.

### Tasks

1. Install Redis in `Vessel.Infrastructure`.

```powershell
dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj package StackExchange.Redis
```

2. Create booking DTOs.

- `CreateBookingDto`
- `BookingResponseDto`
- `UpdateBookingStatusDto`

3. Create `IBookingService` interface.

4. Create `MockBookingService` and register it in DI.

5. Create `BookingsController` with full attribute routing.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Bookings")]`

6. Implement these endpoints with route constraints.

- `[HttpPost]` → `POST /api/bookings` — `[Authorize(Policy = "ConsumerOnly")]`
- `[HttpGet("my-bookings")]` → `GET /api/bookings/my-bookings` — `[Authorize(Policy = "ConsumerOnly")]`
- `[HttpGet("requests")]` → `GET /api/bookings/requests` — `[Authorize(Policy = "ProviderOnly")]`
- `[HttpPatch("{id:guid}/status")]` → `PATCH /api/bookings/{id}/status` — `[Authorize(Policy = "ProviderOnly")]`

7. Add `[ProducesResponseType]` annotations on every action.

8. Document the `Idempotency-Key` header requirement.

- Add `[FromHeader(Name = "Idempotency-Key")] string idempotencyKey` as a parameter on the POST action.
- Swagger will auto-document this header field.

9. Verify the full contract in Swagger UI using the mock.

10. Implement real `BookingService`. Swap the mock registration.

11. Implement booking creation in this order.

- read the authenticated consumer id
- read the `Idempotency-Key`
- check Redis for a cached response
- check the database for an existing booking with the same `ConsumerId` and `IdempotencyKey`
- load the active provider rate for the requested area
- calculate `PricePerGallonSnapshot`
- calculate `TotalPrice`
- create the booking
- save it
- cache the response in Redis

12. Use the database unique index as the source of truth for idempotency.

13. Keep allowed booking status transitions simple for v1.

- `Pending -> Confirmed`
- `Pending -> Cancelled`
- `Confirmed -> Cancelled`

14. Allow providers to update only bookings assigned to them.

15. Add tests for:

- duplicate requests with the same consumer and same key return the original booking
- the same key from a different consumer does not collide
- booking totals do not change after later rate updates
- one provider cannot update another provider's booking

### Done When

- Duplicate submissions do not create duplicate bookings
- Booking pricing is stored as a snapshot
- Provider ownership rules are enforced
- Swagger documents the `Idempotency-Key` header on the POST endpoint

## Phase 7: Price Alerts And Background Jobs

### Tasks

1. Install Hangfire packages in `Vessel.API`.

```powershell
dotnet add Vessel.API/Vessel.API.csproj package Hangfire.AspNetCore
dotnet add Vessel.API/Vessel.API.csproj package Hangfire.PostgreSql
```

2. Create alert DTOs.

- `CreatePriceAlertDto`
- `PriceAlertDto`
- `UpdatePriceAlertDto`

3. Create `IPriceAlertService` interface and implement it directly (no mock needed — by this phase the DI pattern is well-established).

4. Create `AlertsController` with full attribute routing.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Price Alerts")]`
- All endpoints require `[Authorize(Policy = "ConsumerOnly")]`.

5. Implement these endpoints with route constraints.

- `[HttpPost]` → `POST /api/alerts`
- `[HttpGet("my-alerts")]` → `GET /api/alerts/my-alerts`
- `[HttpPatch("{id:guid}")]` → `PATCH /api/alerts/{id}`
- `[HttpDelete("{id:guid}")]` → `DELETE /api/alerts/{id}`

6. Add `[ProducesResponseType]` annotations and XML comments.

7. Register `IPriceAlertService` in `ServiceCollectionExtensions`.

8. Register Hangfire in `Program.cs`.

9. Create `AlertTriggerJob` in `Vessel.Infrastructure/BackgroundJobs`.

10. Schedule the job to run daily.

11. In the job:

- load active alerts
- load current active rates for the same areas
- calculate the total for the alert volume
- compare the total against the alert direction and threshold
- skip alerts already triggered by the same rate row
- send SignalR notifications
- update `LastTriggeredRateId`

12. Do not implement email notifications in v1. Use SignalR as the only notification channel.

13. Expose the Hangfire dashboard at `/hangfire` and protect it with admin-only access.

14. Add tests for:

- above-threshold alerts
- below-threshold alerts
- duplicate notifications are not sent on every job run for the same rate

### Done When

- Consumers can create and manage alerts
- Alerts trigger from the recurring job
- Duplicate notifications are suppressed for unchanged rates
- All alert endpoints are documented in Swagger

## Phase 8: Admin Analytics

### Tasks

1. Create analytics DTOs.

- `TopProviderDto`
- `AveragePriceDto`
- `VolumeTrendDto`
- `PriceTrendDto`

2. Create `IAdminAnalyticsService` interface and implement it directly.

3. Create `AnalyticsController` with full attribute routing.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Analytics")]`
- `[Authorize(Policy = "AdminOnly")]` on the controller class.

4. Implement these endpoints with query-parameter binding.

- `[HttpGet("top-providers")]` → `GET /api/analytics/top-providers`
- `[HttpGet("average-prices")]` → `GET /api/analytics/average-prices?city={city}` — bind `[FromQuery] string city`.
- `[HttpGet("volume-trends")]` → `GET /api/analytics/volume-trends?days={days}` — bind `[FromQuery] int days = 30`.
- `[HttpGet("price-trends")]` → `GET /api/analytics/price-trends?areaId={areaId}&days={days}` — bind `[FromQuery] Guid areaId`, `[FromQuery] int days = 30`.

5. Add `[ProducesResponseType]` annotations and XML comments to every action.

6. Register `IAdminAnalyticsService` in `ServiceCollectionExtensions`.

7. Use these data sources.

- top providers: booking counts
- average prices: current active provider rates
- volume trends: bookings grouped by day
- price trends: provider rate history grouped by time window

8. Protect all analytics endpoints with the `AdminOnly` policy.

9. Add tests for:

- admin-only access
- aggregation results on seeded test data

### Done When

- All analytics endpoints return the correct data
- Non-admin users are blocked
- Swagger documents all query parameters with defaults

## Phase 9: AI And RAG

### Tasks

1. Install AI packages after phases 1–8 are complete and the core backend is stable.

```powershell
dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj package pgvector.EntityFrameworkCore
dotnet add Vessel.AI/Vessel.AI.csproj package Microsoft.SemanticKernel
dotnet add Vessel.AI/Vessel.AI.csproj package Microsoft.SemanticKernel.Connectors.OpenAI
```

2. Use OpenAI as the model provider. Store the API key in `appsettings.Development.json` under `OpenAI:ApiKey`.

3. Create `RateEmbedding` in `Vessel.Core/Entities`.

- `Id`
- `SourceType`
- `SourceId`
- `ContentText`
- `Embedding`
- `CreatedAt`

4. Create an enum for `SourceType`.

- `ProviderRate`
- `PriceAlert`

5. Create the EF Core configuration for `RateEmbedding`.

6. Add a new migration for the AI schema.

7. In that migration:

- enable the PostgreSQL `vector` extension
- create the `RateEmbeddings` table

8. Create services in `Vessel.AI`.

- `IEmbeddingGeneratorService`
- `IRagQueryService`

9. Generate embeddings for:

- provider rate changes
- alert events

10. Do not ingest external news in v1. Generate embeddings only from Vessel's own rate and alert data.

11. Generate embeddings asynchronously so normal rate updates are not blocked by AI calls.

12. Create `AiInsightsController`.

13. Implement this endpoint.

- `POST /api/ai/ask`

14. In the RAG flow:

- embed the user question
- retrieve the top matching rows from `RateEmbeddings`
- build a prompt from the retrieved context
- ask the model for a final answer
- return the answer along with the supporting `RateEmbedding` ids used in generation

15. Add tests for:

- embedding records are created
- similar questions retrieve relevant context
- rate updates still succeed when embedding generation is queued asynchronously

### Done When

- Embeddings are stored in PostgreSQL
- Natural-language questions can retrieve and summarize market data
- Core write flows do not depend on live AI latency

## Phase 10: Hardening And Delivery

### Tasks

1. Add request validation for all input DTOs using a FluentValidation action filter registered globally in `Program.cs`.

2. Add global exception handling middleware.

3. Return consistent `ProblemDetails` responses for all error codes (400, 401, 403, 404, 409, 500).

4. Add ASP.NET Core rate limiting.

Required v1 policy:

- `30 requests per minute per IP`

5. Add health checks for:

- PostgreSQL
- Redis

6. Add structured logging.

7. Final Swagger / OpenAPI audit.

- Confirm every controller has `[Tags]`.
- Confirm every action has `[ProducesResponseType]` for success and error codes.
- Confirm every action has XML `<summary>` and `<param>` comments.
- Confirm the JWT "Authorize" button works end-to-end.
- Confirm query parameters render with descriptions and defaults.
- Confirm the `Idempotency-Key` header shows on the booking POST endpoint.
- Export the OpenAPI JSON and verify it is well-formed.

8. Final DI audit.

- Confirm all mock registrations have been replaced with real implementations.
- Confirm no controller depends on a concrete class.
- Confirm `ServiceCollectionExtensions` is the single place where all services are registered.
- Move any remaining mock classes to `Vessel.Tests/Mocks/` for integration test use. Delete them from `Vessel.Infrastructure/Mocks/`.

9. Final routing audit.

- Confirm every controller uses `[Route("api/[controller]")]` and `[ApiController]`.
- Confirm every route parameter has a type constraint (`:guid`, `:int`, etc.).
- Confirm no conventional routing is registered in `Program.cs`.

10. Add a Dockerfile for `Vessel.API`.

11. Add a GitHub Actions workflow for:

- restore
- build
- test

12. Expand test coverage for:

- auth
- rate creation
- booking idempotency
- alert jobs
- analytics

13. Run the full test suite before moving to frontend work.

### Done When

- Validation and error handling are consistent
- Rate limiting is active
- Health checks work
- CI builds and tests the backend
- Swagger is fully documented and audited
- No mock services remain in production DI registrations
- All routes use attribute routing with constraints

## Suggested Folder Layout

```text
backend/
  Vessel.sln
  docker-compose.yml
  implementation_plan.md
  Vessel.Core/
    Entities/
    Enums/
    Exceptions/
  Vessel.Application/
    DTOs/
    Interfaces/
    Services/
    Validators/
  Vessel.Infrastructure/
    Data/
    Data/Configurations/
    Data/Migrations/
    Repositories/
    BackgroundJobs/
    Caching/
    Auth/
    Mocks/              ← mock/stub service implementations (temporary)
  Vessel.AI/
    Services/
    Configuration/
  Vessel.API/
    Controllers/
    Hubs/
    Middleware/
    Extensions/
    Program.cs
    appsettings.json
    appsettings.Development.json
    Vessel.API.csproj    ← has <GenerateDocumentationFile>true</GenerateDocumentationFile>
  Vessel.Tests/
    Unit/
    Integration/
```
