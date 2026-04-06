# Vessel Backend Implementation Plan

## Status Snapshot

- Current repo state: `backend/` currently contains only this plan file. The solution and all .NET projects still need to be created.
- Handoff goal: another chat should be able to resume work from this file without re-deciding architecture, data contracts, or environment setup.
- Working directory for every command below unless stated otherwise: `d:\Projects\vessel\backend`

## Progress Tracker

| Phase | Status | Notes |
|---|---|---|
| 1. Bootstrap And Local Setup | Completed | Verified build, docker, health-check, secrets, and Swagger. |
| 2. Domain Model And Persistence | Completed | Full schema migrated, audited, and seeded with test data. |
| 3. Authentication And Authorization | Not started | |
| 4. Areas And Rate Intelligence | Not started | |
| 5. Provider Discovery | Not started | |
| 6. Booking Engine And Idempotency | Not started | |
| 7. Price Alerts And Background Jobs | Not started | |
| 8. Admin Analytics | Not started | |
| 9. AI And RAG | Not started | |
| 10. Hardening And Delivery | Not started | |

Update this table whenever a phase starts or finishes.

## Working Setup

- Target framework: `net10.0`
- Pin the exact installed SDK with `global.json` after the first successful local build.
- Backend style for v1: ASP.NET Core 10 Web API with controllers only
- Database: Supabase PostgreSQL for normal development, local `postgres-test` only for isolated integration tests
- Vector store: `pgvector` in PostgreSQL
- Cache: Redis
- AI provider: Google Gemini via Semantic Kernel
- Real-time: SignalR
- Background jobs: Hangfire
- Tests: xUnit
- Secrets: use `dotnet user-secrets` or environment variables for local secrets. Do not commit real connection strings, JWT keys, seed passwords, or Gemini keys into `appsettings*.json`.

## Working Rules

- Use controllers for v1. Do not mix minimal APIs into the same backend while the core flows are still being built.
- Keep domain entities, enums, and domain exceptions in `Vessel.Core`.
- Keep DTOs, validators, service interfaces, and repository/query interfaces in `Vessel.Application`.
- Keep EF Core, Redis, Hangfire, SignalR infrastructure, auth persistence, and external integrations in `Vessel.Infrastructure`.
- Keep Semantic Kernel orchestration and embedding logic in `Vessel.AI`.
- `Vessel.API` is the composition root. Controllers must not use `DbContext` directly.
- Do not add a generic repository. Add small feature-specific repository/query abstractions only when a use case needs them.
- Every concrete dependency is resolved through an interface.
- Public registration creates `Consumer` users only.
- Provider and Admin accounts are provisioned manually or via Development-only seed data in v1.
- Use UTC everywhere.
- Keep all I/O async.
- Return plain DTOs on success and `ProblemDetails` on failures. Do not introduce a generic response envelope in v1.

### Routing, Validation, And Status Codes

- Use attribute routing on every controller. Do not rely on conventional routing.
- Decorate each controller class with `[Route("api/[controller]")]` and `[ApiController]` unless a different explicit route is clearly justified.
- Decorate each action with explicit HTTP-method attributes such as `[HttpGet]`, `[HttpPost]`, `[HttpPatch]`, and `[HttpDelete]`.
- Apply route constraints on path parameters wherever the type is known, for example `[HttpGet("{id:guid}")]`.
- For multi-parameter query inputs that need validation, bind a request DTO with `[FromQuery]` instead of separate scalar parameters.
- Reserve direct scalar `[FromQuery]` parameters for very simple inputs that only rely on built-in type binding.
- Register the validation pipeline in Phase 1 so validators added in later phases are automatically enforced.
- Collection endpoints return `200 OK` with an empty array when the parent resource exists but has no rows.
- Use `404 Not Found` only when the addressed resource does not exist, for example an unknown `areaId` or `providerId`.
- Resource creation endpoints return `201 Created`.
- Use `409 Conflict` for uniqueness or idempotency conflicts.
- Never expose an endpoint without an explicit route template.

### Swagger / OpenAPI

- Swagger UI must be available from Phase 1 onward. It is the primary way to demo and test the API before a frontend exists.
- Use `Swashbuckle.AspNetCore` (v10+) for OpenAPI document generation. 
- **Important**: Do not install `Microsoft.OpenApi` separately; Swashbuckle brings the correct v2.x version.
- Use the root `Microsoft.OpenApi` namespace for all models; the `.Models` sub-namespace is removed in v2.x.
- Add XML doc comments (`<summary>`, `<param>`, `<response>`) to every controller action. Enable XML comment generation in the `.csproj` to avoid build warnings.
- Tag controllers with `[Tags("...")]` so Swagger groups endpoints logically.
- Configure Swagger to include the JWT bearer scheme using `OpenApiSecuritySchemeReference`.
- Add `[ProducesResponseType]` attributes on every action to document expected status codes.

### Dependency Injection Strategy (Mock-First)

- Every service and repository is consumed through its interface. Concrete classes are never injected directly.
- In early phases, register mock or stub implementations for services that do not exist yet. This lets controllers compile, Swagger show the intended API surface, and integration tests run against predictable data.
- As each phase is completed, swap the mock registration for the real implementation in `ServiceCollectionExtensions`. Controller code should not change.
- Progression pattern per phase:
  1. Define the interface in `Vessel.Application`.
  2. Create a `Mock___Service` in `Vessel.Infrastructure/Mocks` that returns hard-coded in-memory data.
  3. Register the mock in DI.
  4. Build and test the controller against the mock.
  5. Implement the real service.
  6. Replace the mock registration with the real one.
  7. Move the mock class to `Vessel.Tests/Mocks/` for integration test use and remove it from the production project.
- Store mock implementations in `Vessel.Infrastructure/Mocks/`.

## V1 Clarifications To Prevent Rework

- Provider discovery returns one row per active provider-area pair, not one row per provider company.
- `distanceKm` in discovery results means user-to-area-centroid distance, not provider depot distance, because provider coordinates are not part of the v1 model.
- Price alerts compare a total price for a chosen volume, not a raw price-per-gallon threshold. Use the field name `ThresholdTotalPrice` instead of `ThresholdPrice`.
- Rate change notifications can be global broadcasts in v1. Alert-trigger notifications must be user-targeted with SignalR so one consumer does not receive another consumer's alert.
- AI in v1 uses only Vessel's internal data such as rates and alert events. External news ingestion mentioned in the PRD is intentionally postponed.
- No pagination is planned for v1 list endpoints. Return stable sort orders instead:
  - bookings and alerts: newest first
  - rates: cheapest first, then provider name
  - provider discovery: nearest first, then cheapest

## Solution Layout

| Project | Type | Purpose |
|---|---|---|
| `Vessel.Core` | Class library | Entities, enums, domain exceptions |
| `Vessel.Application` | Class library | DTOs, validators, service interfaces, repository/query interfaces, business rules |
| `Vessel.Infrastructure` | Class library | EF Core, repository/query implementations, Redis, auth persistence, jobs |
| `Vessel.API` | ASP.NET Core Web API | Controllers, middleware, DI, auth, SignalR, Swagger |
| `Vessel.AI` | Class library | Embeddings, RAG orchestration, prompt handling |
| `Vessel.Tests` | xUnit | Unit tests and integration tests |

## Cross-Cutting Technical Decisions

### Data And Entity Conventions

- Use `Guid` primary keys for all aggregate roots and child records.
- Persist timestamps as UTC and use `DateTimeOffset` in the entity model to avoid ambiguous local time handling.
- `UpdatedAt` is non-null and is initialized to the same value as `CreatedAt` on insert.
- Nullable fields:
  - `RefreshToken.RevokedAt`
  - `RefreshToken.ReplacedByTokenHash`
  - `ProviderRate.EffectiveTo`
  - `Booking.Notes`
  - `PriceAlert.LastTriggeredRateId`
- Monetary precision:
  - `PricePerGallon`, `PricePerGallonSnapshot`, `ThresholdTotalPrice`: `decimal(18,4)`
  - `TotalPrice`: `decimal(18,2)`
- `VolumeInGallons` should be `int` in v1.
- `Latitude` and `Longitude` should be `double`.
- Required max lengths:
  - `Email`: 320
  - `FullName`: 200
  - `CompanyName`: 200
  - `ContactNumber`: 50
  - `City`: 100
  - `Area.Name`: 150
  - `DeliveryAddress`: 500
  - `Notes`: 1000
- `Provider` has its own `Id` and a unique `UserId`. `Booking.ProviderId` points to `Provider.Id`, not `User.Id`.
- `Booking.ConsumerId` and `PriceAlert.ConsumerId` point to `User.Id`.
- Use `DeleteBehavior.Restrict` for historical or business records so deleting a user or provider cannot wipe bookings, alerts, or rate history. Cascade only from `User` to `RefreshTokens`.
- Seed IDs should be deterministic and stored in a central `SeedIds` helper so tests and future chats can reference the same IDs.

### Auth, Claims, And SignalR

- Access tokens must include `sub`, `email`, and `role` claims.
- Use the authenticated user id from `sub` or `ClaimTypes.NameIdentifier` as the single source of truth in controllers and SignalR.
- Map SignalR user identity to the authenticated user id so alert notifications can be sent through `Clients.User(userId)`.
- Public registration is consumer-only. Provider onboarding is manual or admin-driven in v1.

### Testing And Operational Conventions

- Integration tests must use the local `postgres-test` container and local Redis. Do not point tests at Supabase.
- Add a `DesignTimeDbContextFactory` in `Vessel.Infrastructure` so EF migrations work consistently even before the full API host is stable.
- Use Hangfire fire-and-forget jobs for asynchronous embedding generation in Phase 9.
- The validation pipeline is established in Phase 1. Phase 10 is an audit and completion phase, not the first time validation is wired up.
- Idempotency contract:
  - same authenticated consumer + same `Idempotency-Key` + equivalent payload -> return the original booking response
  - same authenticated consumer + same `Idempotency-Key` + different payload -> return `409 Conflict`
  - different consumer + same key -> no collision

## Phase 1: Bootstrap And Local Setup

### Tasks

1. Change into the backend directory.

```powershell
Set-Location d:\Projects\vessel\backend
```

2. Verify local prerequisites and record the exact SDK version that will later be pinned in `global.json`.

```powershell
dotnet --version
docker --version
docker compose version
```

3. Create the solution and projects from `backend/`.

```powershell
dotnet new sln -n Vessel

dotnet new classlib -n Vessel.Core -o Vessel.Core -f net10.0
dotnet new classlib -n Vessel.Application -o Vessel.Application -f net10.0
dotnet new classlib -n Vessel.Infrastructure -o Vessel.Infrastructure -f net10.0
dotnet new classlib -n Vessel.AI -o Vessel.AI -f net10.0
dotnet new webapi -n Vessel.API -o Vessel.API -f net10.0 --use-controllers
dotnet new xunit -n Vessel.Tests -o Vessel.Tests -f net10.0
```
4. Add all projects to the solution.

```powershell
dotnet sln Vessel.slnx add Vessel.Core/Vessel.Core.csproj
dotnet sln Vessel.slnx add Vessel.Application/Vessel.Application.csproj
dotnet sln Vessel.slnx add Vessel.Infrastructure/Vessel.Infrastructure.csproj
dotnet sln Vessel.slnx add Vessel.AI/Vessel.AI.csproj
dotnet sln Vessel.slnx add Vessel.API/Vessel.API.csproj
dotnet sln Vessel.slnx add Vessel.Tests/Vessel.Tests.csproj
```

5. Add project references.

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

6. Install the first packages.

```powershell
dotnet add Vessel.Application/Vessel.Application.csproj package FluentValidation

dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL

dotnet add Vessel.API/Vessel.API.csproj package Swashbuckle.AspNetCore
# Note: Do NOT add Microsoft.OpenApi manually. Swashbuckle v10 brings its own v2.x dependency.

dotnet add Vessel.Tests/Vessel.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add Vessel.Tests/Vessel.Tests.csproj package FluentAssertions
```

7. Install the EF Core CLI tool.

```powershell
dotnet tool update --global dotnet-ef
```

8. After the first successful build, pin the exact SDK with `global.json`.

```powershell
dotnet new globaljson --sdk-version <exact-dotnet-version-from-step-2>
```

9. Create `backend/docker-compose.yml`.

Note: use this for local auxiliary services and isolated integration tests. Supabase remains the primary database for normal development.

```yaml
services:
  redis:
    image: redis:7-alpine
    container_name: vessel-redis
    ports:
      - "6379:6379"

  postgres-test:
    image: pgvector/pgvector:pg16
    container_name: vessel-postgres-test
    environment:
      POSTGRES_DB: vessel_test
      POSTGRES_USER: vessel
      POSTGRES_PASSWORD: vessel
    ports:
      - "5433:5432"
```

10. Start local services.

```powershell
docker compose up -d
```

11. Initialize local secrets for `Vessel.API`.

```powershell
dotnet user-secrets init --project Vessel.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SUPABASE_POSTGRES_CONNECTION_STRING" --project Vessel.API
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379" --project Vessel.API

dotnet user-secrets set "Jwt:Issuer" "Vessel" --project Vessel.API
dotnet user-secrets set "Jwt:Audience" "Vessel.Client" --project Vessel.API
dotnet user-secrets set "Jwt:Key" "replace-this-with-a-long-development-secret-key" --project Vessel.API
dotnet user-secrets set "Jwt:AccessTokenMinutes" "60" --project Vessel.API
dotnet user-secrets set "Jwt:RefreshTokenDays" "7" --project Vessel.API

dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_FREE_API_KEY" --project Vessel.API
dotnet user-secrets set "Gemini:ChatModelId" "gemini-2.5-flash" --project Vessel.API
dotnet user-secrets set "Gemini:EmbeddingModelId" "VERIFY_IN_PHASE_9" --project Vessel.API
```

12. Keep only non-secret defaults and placeholders for documentation in `Vessel.API/appsettings.json`.

Recommended non-secret shape with secret placeholders:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host;Database=postgres;Username=postgres;Password=YOUR_SECURE_PASSWORD;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Issuer": "Vessel",
    "Audience": "Vessel.Client",
    "Key": "PLACEHOLDER_FOR_32_CHAR_SECRET_KEY",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "ChatModelId": "gemini-2.5-flash",
    "EmbeddingModelId": "VERIFY_IN_PHASE_9"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  }
}
```

13. Delete template files from `Vessel.API`.

- `WeatherForecast.cs`
- `WeatherForecastController.cs`

14. Clean `Program.cs` so only required baseline registrations remain.

Required baseline:

- `AddControllers()`
- `AddEndpointsApiExplorer()`
- `AddSwaggerGen(...)`
- `AddCors(...)`
- `MapControllers()`
- `UseHttpsRedirection()`

15. Configure Swagger in `Program.cs`.

- Use `using Microsoft.OpenApi;` (Note: `Microsoft.OpenApi.Models` is deprecated in v2.x/Swashbuckle v10).
- Register `AddSwaggerGen()` with API info:
  - title: `Vessel API`
  - version: `v1`
- Add a JWT bearer security definition using `OpenApiSecurityScheme`.
- **Note**: In Swashbuckle v10, use `options.AddSecurityRequirement(_ => securityRequirement)` (lambda syntax).
- Within the requirement, use `new OpenApiSecuritySchemeReference("Bearer")` to link to the definition.
- Enable `UseSwagger()` and `UseSwaggerUI()` in Development.
- Enable XML comments by adding `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to `Vessel.API.csproj` and passing the XML path to `SwaggerGenOptions.IncludeXmlComments()`.
- **Warning**: Ensure all public controller members have XML comments (`///`) to avoid build warnings once enabled.

16. Create `Vessel.API/Extensions/ServiceCollectionExtensions.cs`.

- This file will hold all DI registrations grouped by feature area.
- Start with a method `AddApplicationServices(this IServiceCollection services)` that registers nothing yet. Each phase will add to it.
- Call it from `Program.cs`.

17. Add the validation pipeline baseline.

- Create a global validation filter or equivalent MVC pipeline hook now.
- The goal is for future FluentValidation validators to be enforced without per-controller plumbing.
- Keep actual validator coverage feature-by-feature in later phases.

18. Configure CORS in `Program.cs`.

- Define a policy such as `VesselFrontend`.
- Allow `http://localhost:5173`.
- Allow any header and any method.
- Use `app.UseCors("VesselFrontend")` before authentication and authorization middleware.

19. Create a `HealthController` as a smoke-test endpoint.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Health")]`
- `[HttpGet]` returns `200 OK` with `{ "status": "healthy" }`
- Add `[ProducesResponseType(typeof(object), 200)]`

20. Build the solution.

```powershell
dotnet build
```

21. Run the API once to confirm the solution starts.

```powershell
dotnet run --project Vessel.API
```

22. Open Swagger at the exact local URL printed by `dotnet run`, then append `/swagger`.

### Done When

- `dotnet build` succeeds
- `global.json` exists
- local Redis and `postgres-test` are running
- `Vessel.API` starts without template errors
- Swagger UI loads and shows the health endpoint
- `ServiceCollectionExtensions.cs` exists and is called from `Program.cs`
- secrets are stored via user-secrets instead of committed config values

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
  - `UpdatedAt`

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
  - `ThresholdTotalPrice`
  - `TargetVolumeInGallons`
  - `Direction`
  - `IsActive`
  - `LastTriggeredRateId`
  - `CreatedAt`
  - `UpdatedAt`

4. Do not create `RateEmbedding` yet. Add it in Phase 9 with the AI migration.

5. Create `ApplicationDbContext` in `Vessel.Infrastructure/Data`.

6. Create `ApplicationDbContextFactory` in `Vessel.Infrastructure/Data` for design-time migrations and test stability.

7. Create these folders in `Vessel.Application`.

- `Interfaces/Repositories/`
- `Interfaces/Queries/`

8. Add only the repository interfaces you need for the first backend features.

Start with:

- `IUserRepository`
- `IRefreshTokenRepository`
- `IAreaRepository`
- `IProviderRepository`
- `IProviderRateRepository`
- `IBookingRepository`
- `IPriceAlertRepository`

9. Implement those interfaces in `Vessel.Infrastructure`.

10. Create one EF Core configuration class per entity in `Vessel.Infrastructure/Data/Configurations`.

11. Add database constraints and indexes.

- Unique index on `Users.Email`
- Unique index on `Areas.City + Areas.Name`
- Unique index on `Providers.UserId`
- Unique index on `Bookings.ConsumerId + IdempotencyKey`
- Filtered unique index on `ProviderRates.ProviderId + AreaId` where `EffectiveTo IS NULL`

12. Set explicit precision and nullability.

- `PricePerGallon`, `PricePerGallonSnapshot`, and `ThresholdTotalPrice`: `decimal(18,4)`
- `TotalPrice`: `decimal(18,2)`
- `Notes`, `EffectiveTo`, `RevokedAt`, `ReplacedByTokenHash`, and `LastTriggeredRateId` are nullable

13. Configure delete behaviors.

- `User -> RefreshTokens`: cascade
- All historical and business relationships: restrict

14. Configure all timestamps in UTC and centralize `CreatedAt` / `UpdatedAt` stamping in the `DbContext` save pipeline or an interceptor.

15. Seed a starter list of areas covering Islamabad, Lahore, and Karachi using deterministic GUIDs.

Implementation rule:

- once the final area names and coordinates are chosen, keep them stable and record the resulting GUIDs in both code and this document
- do not build admin area management in v1

16. Seed Development-only users for one admin and one provider.

Important:

- create both the `User` row and the matching `Provider` row for the seeded provider account
- keep seed passwords out of source control by reading them from configuration or user-secrets

17. Create the initial migration.

```powershell
dotnet ef migrations add InitialCreate --project Vessel.Infrastructure --startup-project Vessel.API --output-dir Data/Migrations
```

18. Apply the migration.

```powershell
dotnet ef database update --project Vessel.Infrastructure --startup-project Vessel.API
```

Note:

- `--project` is the project that contains the `DbContext`
- `--startup-project` is the executable project that loads configuration

### Done When
- The schema exists in PostgreSQL
- All entities map correctly
- Seeded areas and Development-only seed users are inserted
- The unique constraints are enforced by the database
- The filtered active-rate constraint works
- Migrations can run through the design-time factory without depending on fragile startup behavior

### Post-Implementation Sync & Troubleshooting Notes (Phase 2)

> [!NOTE]
> **Database Seeding Distinction:**
> - **Schema & Areas**: Seeded via formal **EF Core Migrations** (`HasData`). Visible in Supabase Migrations UI.
> - **Test Users (Admin/Provider)**: Seeded via **Direct SQL (MCP)** (`execute_sql`). **NOT** visible in Supabase Migrations UI, but data is present in the `Users` and `Providers` tables.
>
> **Connection Requirements (Local to Supabase):**
> - **Secrets**: The following must be set in `dotnet user-secrets` for the API project:
>   - `SeedConfigs:AdminPassword`
>   - `SeedConfigs:ProviderPassword`
>   - `ConnectionStrings:DefaultConnection`
> - **SSL**: Ensure your connection string includes `SSL Mode=Require` to connect to Supabase.
>
> **Hashing Strategy:**
> - Uses **BCrypt.Net-Next** (Standard Cost 11).
> - Seeded passwords are currently `Admin123!` and `Provider123!`.

- [x] Phase 2: Completed and Verified. Schema and data are fully synced on Supabase cloud.

## Phase 3: Authentication And Authorization

### Tasks

1. Install the auth package in `Vessel.API`. From the backend folder.

```powershell
dotnet add Vessel.API/Vessel.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
```

2. Create these folders in `Vessel.Application`.

- `DTOs/Auth/`
- `Interfaces/Auth`
- `Services/`

3. Create auth DTOs.

- `RegisterRequestDto`
  - `Email`
  - `Password`
  - `FullName`
- `LoginRequestDto`
  - `Email`
  - `Password`
- `RefreshTokenRequestDto`
  - `RefreshToken`
- `AuthResponseDto`
  - `AccessToken`
  - `AccessTokenExpiresAtUtc`
  - `RefreshToken`
  - `RefreshTokenExpiresAtUtc`
  - `UserId`
  - `Role`

4. Create service interfaces.

- `IAuthService`
- `IJwtTokenService`
- `IPasswordHasherService`

5. Implement auth services.

- implement `AuthService` in `Vessel.Application/Services`
- implement `JwtTokenService` in `Vessel.Infrastructure/Services/Auth` (create missing folders along the way)
- implement `PasswordHasherService` in `Vessel.Infrastructure/Services/Auth`
- hash passwords securely
- create access tokens
- create refresh tokens
- store only refresh token hashes in the database
- revoke old refresh tokens on refresh
- use config-driven token lifetimes:
  - access token: 60 minutes
  - refresh token: 7 days

6. Configure custom JWT authentication in `Program.cs`.

Note: Supabase hosts PostgreSQL, but identity and JWT issuance are owned by the .NET backend.

- Read `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`, `Jwt:AccessTokenMinutes`, and `Jwt:RefreshTokenDays` from configuration.
- Register `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` with `AddJwtBearer()`.
- Validate issuer, audience, signing key, and lifetime.
- Set `RequireHttpsMetadata = false` for local development only.
- Place `UseAuthentication()` before `UseAuthorization()` in the pipeline.

7. Add authorization policies.

- `ConsumerOnly`: requires role `Consumer`
- `ProviderOnly`: requires role `Provider`
- `AdminOnly`: requires role `Admin`
- Register them with `AddAuthorizationBuilder().AddPolicy(...)` in `Program.cs`.

8. Update Swagger configuration to support JWT.

- The security definition from Phase 1 should already be in place.
- Verify the flow in this order: register -> login -> copy the token -> click `Authorize` -> paste `Bearer {token}` -> call a protected endpoint.

9. Create `AuthController` with attribute routing and response annotations.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Authentication")]`
- `[HttpPost("register")]`
  - `[ProducesResponseType(typeof(AuthResponseDto), 201)]`
  - `[ProducesResponseType(400)]`
  - `[ProducesResponseType(409)]`
- `[HttpPost("login")]`
  - `[ProducesResponseType(typeof(AuthResponseDto), 200)]`
  - `[ProducesResponseType(401)]`
- `[HttpPost("refresh")]`
  - `[ProducesResponseType(typeof(AuthResponseDto), 200)]`
  - `[ProducesResponseType(401)]`
- Add XML `<summary>` comments to each action.

10. Register auth services in `ServiceCollectionExtensions`.

- `IAuthService` -> `AuthService`
- `IJwtTokenService` -> `JwtTokenService`
- `IPasswordHasherService` -> `PasswordHasherService`

11. Make public registration create `Consumer` users only.

12. Keep provider and admin account creation out of the public API in v1.

13. Seed one admin account and one provider account for testing in Development only.

14. Add validators for the auth request DTOs.

15. Add tests for:

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
- Provider and admin accounts are not publicly self-registrable
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
  - `Id`
  - `City`
  - `Name`
  - `Latitude`
  - `Longitude`
- `RateDto`
  - `ProviderId`
  - `CompanyName`
  - `AreaId`
  - `AreaName`
  - `City`
  - `PricePerGallon`
  - `EffectiveFrom`
- `CreateRateDto`
  - `AreaId`
  - `PricePerGallon`
- `RateHistoryDto`
  - `PricePerGallon`
  - `EffectiveFrom`
  - `EffectiveTo`

3. Create service interfaces.

- `IAreaService`
- `IRateService`

4. Create mock implementations and register them in DI.

- `MockAreaService` -> returns the seeded area list from memory.
- `MockRateService` -> returns a few fake rates.

5. Create controllers with full attribute routing.

- `AreasController` -> `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Areas")]`
- `RatesController` -> `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Rates")]`

6. Implement these endpoints with route constraints.

- `[HttpGet]` on `AreasController` -> `GET /api/areas`
- `[HttpGet("{areaId:guid}")]` on `RatesController` -> `GET /api/rates/{areaId}`
- `[HttpGet("history/{areaId:guid}/{providerId:guid}")]` on `RatesController` -> `GET /api/rates/history/{areaId}/{providerId}`
- `[HttpPost]` on `RatesController` -> `POST /api/rates`

7. Add response annotations to every action.

- `AreasController.GetAll`: `[ProducesResponseType(typeof(List<AreaDto>), 200)]`
- `RatesController.GetByArea`: `[ProducesResponseType(typeof(List<RateDto>), 200)]`, `[ProducesResponseType(404)]` only when `areaId` does not exist
- `RatesController.GetHistory`: `[ProducesResponseType(typeof(List<RateHistoryDto>), 200)]`, `[ProducesResponseType(404)]` only when `areaId` or `providerId` does not exist
- `RatesController.Create`: `[ProducesResponseType(typeof(RateDto), 201)]`, `[ProducesResponseType(400)]`, `[ProducesResponseType(401)]`, `[ProducesResponseType(403)]`, `[ProducesResponseType(409)]`

8. Verify endpoints render correctly in Swagger UI while still backed by mocks.

9. Implement real `AreaService` and `RateService`. Swap mock registrations for real ones in `ServiceCollectionExtensions`.

10. Lock `POST /api/rates` to provider users only using `[Authorize(Policy = "ProviderOnly")]`.

11. In rate creation and update flows, derive the provider from the authenticated user. Do not accept `ProviderId` from the request body.

12. In the rate update flow:

- find the current active rate for that provider and area
- set its `EffectiveTo` to the current UTC time
- insert a new row with the new `PricePerGallon`
- set `EffectiveFrom` to the current UTC time
- save both changes in one transaction

13. Add `RateAlertHub` in `Vessel.API/Hubs` and map it at `/hubs/rates`.

14. After a rate update succeeds, broadcast a `RateChanged` SignalR event with enough data for the frontend to refresh.

Note: v1 Hub strategy is a global broadcast. Future optimization will use `Groups` based on `AreaId`.

15. Add tests for:

- posting a new rate expires the old active rate
- history returns older rates and latest rates correctly
- public rate queries return only active rates
- provider identity is taken from the JWT, not the request body

### Done When

- Providers can create new rates
- Rate history is preserved
- Only one active rate exists per provider-area pair
- SignalR broadcasts on successful rate changes
- Mock services are removed from production DI
- Swagger shows all rate endpoints with full schemas

## Phase 5: Provider Discovery

### Mock-First Approach

Create `MockProviderDiscoveryService` that returns a handful of static providers with pre-calculated distances. This lets you finalize the controller, query parameters, and Swagger docs before implementing Haversine.

### Tasks

1. Create DTOs for provider search.

- `SearchProvidersQueryDto`
  - `Lat`
  - `Lon`
  - `RadiusKm`
- `ProviderSearchResultDto`
  - `ProviderId`
  - `CompanyName`
  - `AreaId`
  - `AreaName`
  - `City`
  - `DistanceKm`
  - `CurrentPricePerGallon`

2. Create service interface.

- `IProviderDiscoveryService`

3. Create `MockProviderDiscoveryService` in `Vessel.Infrastructure/Mocks/`. Register it in DI.

4. Create `ProvidersController`.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Providers")]`

5. Implement this endpoint with query-parameter binding.

Use a query DTO instead of individual scalar parameters:

- `GET /api/providers/search?lat={lat}&lon={lon}&radiusKm={radiusKm}`
- bind as `[FromQuery] SearchProvidersQueryDto query`
- keep `[ProducesResponseType(typeof(List<ProviderSearchResultDto>), 200)]`
- keep `[ProducesResponseType(400)]` for invalid coordinates

6. Add a FluentValidation validator for the query DTO.

- `lat` must be between -90 and 90.
- `lon` must be between -180 and 180.
- `radiusKm` must be > 0 and <= 100 (sensible upper bound for v1).

7. Verify the endpoint works in Swagger with the mock.

8. Implement real `ProviderDiscoveryService`. Swap the mock registration.

9. Implement the Haversine formula yourself in C#.

10. Use the area coordinates as the distance source.

11. Query nearby areas, join to active provider rates, and return provider-area pairs that serve those areas.

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

14. Keep the response shape as one row per active provider-area pair. Do not collapse multiple areas belonging to the same provider in v1.

15. Add tests for:

- providers outside the radius are excluded
- sorting is distance first and price second
- invalid coordinates return 400

### Done When

- Geo search returns the correct provider-area results
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
  - `ProviderId`
  - `AreaId`
  - `VolumeInGallons`
  - `ScheduledFor`
  - `DeliveryAddress`
  - `Notes`
- `BookingResponseDto`
  - `Id`
  - `ConsumerId`
  - `ProviderId`
  - `AreaId`
  - `VolumeInGallons`
  - `PricePerGallonSnapshot`
  - `TotalPrice`
  - `DeliveryAddress`
  - `Notes`
  - `Status`
  - `ScheduledFor`
  - `CreatedAt`
  - `UpdatedAt`
- `UpdateBookingStatusDto`
  - `Status`

3. Create `IBookingService` interface.

4. Create `MockBookingService` and register it in DI.

5. Create `BookingsController` with full attribute routing.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Bookings")]`

6. Implement these endpoints with route constraints.

- `[HttpPost]` -> `POST /api/bookings` -> `[Authorize(Policy = "ConsumerOnly")]`
- `[HttpGet("my-bookings")]` -> `GET /api/bookings/my-bookings` -> `[Authorize(Policy = "ConsumerOnly")]`
- `[HttpGet("requests")]` -> `GET /api/bookings/requests` -> `[Authorize(Policy = "ProviderOnly")]`
- `[HttpPatch("{id:guid}/status")]` -> `PATCH /api/bookings/{id}/status` -> `[Authorize(Policy = "ProviderOnly")]`

7. Add `[ProducesResponseType]` annotations on every action.

8. Document the `Idempotency-Key` header requirement.

- Add `[FromHeader(Name = "Idempotency-Key")] string idempotencyKey` as a parameter on the POST action.
- Swagger will auto-document this header field.

9. Verify the full contract in Swagger UI using the mock.

10. Implement real `BookingService`. Swap the mock registration.

11. Validate booking creation with these rules:

- `ProviderId` and `AreaId` must exist
- the provider must have an active rate for that area
- `VolumeInGallons` must be positive
- `ScheduledFor` must not be in the past
- `DeliveryAddress` is required

12. Implement booking creation in this order.

- read the authenticated consumer id
- read the `Idempotency-Key`
- check Redis for a cached response
- check the database for an existing booking with the same `ConsumerId` and `IdempotencyKey`
- if the key already exists for that consumer with a different payload, return `409 Conflict`
- load the active provider rate for the requested provider and area
- calculate `PricePerGallonSnapshot`
- calculate `TotalPrice`
- create the booking
- save it
- cache the response in Redis

13. Use the database unique index as the source of truth for idempotency.

14. Cache idempotent booking responses in Redis for 24 hours.

15. Keep allowed booking status transitions simple for v1.

- `Pending -> Confirmed`
- `Pending -> Cancelled`
- `Confirmed -> Cancelled`

16. Allow providers to update only bookings assigned to them.

17. Return booking lists newest first.

18. Add tests for:

- duplicate requests with the same consumer and same key return the original booking
- the same key from a different consumer does not collide
- the same key with a different payload returns `409`
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
  - `AreaId`
  - `ThresholdTotalPrice`
  - `TargetVolumeInGallons`
  - `Direction`
- `PriceAlertDto`
  - `Id`
  - `AreaId`
  - `ThresholdTotalPrice`
  - `TargetVolumeInGallons`
  - `Direction`
  - `IsActive`
  - `LastTriggeredRateId`
  - `CreatedAt`
  - `UpdatedAt`
- `UpdatePriceAlertDto`
  - `ThresholdTotalPrice`
  - `TargetVolumeInGallons`
  - `Direction`
  - `IsActive`

3. Create `IPriceAlertService` and implement it directly. No mock is needed by this phase.

4. Create `AlertsController` with full attribute routing.

Clarification:

- implement `IPriceAlertService` directly in this phase
- do not add a mock service for alerts

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Price Alerts")]`
- All endpoints require `[Authorize(Policy = "ConsumerOnly")]`.

5. Implement these endpoints with route constraints.

- `[HttpPost]` -> `POST /api/alerts`
- `[HttpGet("my-alerts")]` -> `GET /api/alerts/my-alerts`
- `[HttpPatch("{id:guid}")]` -> `PATCH /api/alerts/{id}`
- `[HttpDelete("{id:guid}")]` -> `DELETE /api/alerts/{id}`

6. Add `[ProducesResponseType]` annotations and XML comments.

7. Register `IPriceAlertService` in `ServiceCollectionExtensions`.

8. Register Hangfire in `Program.cs` using the same PostgreSQL database with a dedicated `hangfire` schema.

9. Create `AlertTriggerJob` in `Vessel.Infrastructure/BackgroundJobs`.

10. Schedule the job to run daily in UTC.

11. In the job:

- load active alerts
- load current active rates for the same areas
- calculate the total for the alert volume
- compare the total against the alert direction and threshold
- skip alerts already triggered by the same rate row
- send a targeted SignalR `AlertTriggered` notification to the owning user
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
- Hangfire dashboard is admin-protected

## Phase 8: Admin Analytics

### Tasks

1. Create analytics DTOs.

- `TopProviderDto`
  - `ProviderId`
  - `CompanyName`
  - `ConfirmedBookingCount`
  - `TotalGallons`
- `AveragePriceDto`
  - `City`
  - `AveragePricePerGallon`
  - `ActiveProviderCount`
- `VolumeTrendDto`
  - `Date`
  - `ConfirmedBookingCount`
  - `TotalGallons`
- `PriceTrendDto`
  - `Date`
  - `AveragePricePerGallon`

2. Create `IAdminAnalyticsService` interface and implement it directly.

3. Create `AnalyticsController` with full attribute routing.

- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("Analytics")]`
- `[Authorize(Policy = "AdminOnly")]` on the controller class.

4. Implement these endpoints with query-parameter binding.

- `[HttpGet("top-providers")]` -> `GET /api/analytics/top-providers`
- `[HttpGet("average-prices")]` -> `GET /api/analytics/average-prices?city={city}` -> bind `[FromQuery] string city`.
- `[HttpGet("volume-trends")]` -> `GET /api/analytics/volume-trends?days={days}` -> bind `[FromQuery] int days = 30`.
- `[HttpGet("price-trends")]` -> `GET /api/analytics/price-trends?areaId={areaId}&days={days}` -> bind `[FromQuery] Guid areaId`, `[FromQuery] int days = 30`.

5. Add `[ProducesResponseType]` annotations and XML comments to every action.

6. Register `IAdminAnalyticsService` in `ServiceCollectionExtensions`.

7. Use these data sources for analytics queries.

- **Top providers:** confirmed bookings, all-time in v1
- **Average prices:** active provider rates where `EffectiveTo IS NULL`
- **Volume trends:** confirmed bookings grouped by `CreatedAt` date
- **Price trends:** area-specific provider rate history windowed by `EffectiveFrom`

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

1. Install AI packages only after Phases 1-8 are complete and the core backend is stable.

```powershell
dotnet add Vessel.Infrastructure/Vessel.Infrastructure.csproj package pgvector.EntityFrameworkCore
dotnet add Vessel.AI/Vessel.AI.csproj package Microsoft.SemanticKernel
dotnet add Vessel.AI/Vessel.AI.csproj package Microsoft.SemanticKernel.Connectors.Google
```

Only start this phase after Phases 1-8 are stable.

2. Before finalizing package versions, model IDs, and vector dimensions, verify the current official Google and Semantic Kernel documentation. These details are time-sensitive.

3. Use separate configuration keys for chat and embeddings.

- `Gemini:ChatModelId`
- `Gemini:EmbeddingModelId`

4. Create `RateEmbedding` in `Vessel.Core/Entities`.

- `Id`
- `SourceType`
- `SourceId`
- `ContentText`
- `Embedding`
- `CreatedAt`

5. Create an enum for `SourceType`.

- `ProviderRate`
- `PriceAlert`

6. Create the EF Core configuration for `RateEmbedding`.

7. Add a new migration for the AI schema.

8. In that migration:

- enable the PostgreSQL `vector` extension
- create the `RateEmbeddings` table
- use the embedding dimension documented for the chosen embedding model instead of guessing

9. Create services in `Vessel.AI`.

- `IEmbeddingGeneratorService`
- `IRagQueryService`

10. Generate embeddings for:

- provider rate changes
- alert events

11. Do not ingest external news in v1. Generate embeddings only from Vessel's own rate and alert data.

12. Generate embeddings asynchronously through Hangfire so normal rate updates are not blocked by AI calls.

13. Create `AiInsightsController`.

- `[Route("api/ai")]`, `[ApiController]`, `[Tags("AI")]`
- protect the endpoint with `[Authorize]` to control cost and abuse

14. Implement this endpoint.

- `POST /api/ai/ask`

15. In the RAG flow:

- embed the user question
- retrieve the top matching rows from `RateEmbeddings`
- build a prompt from the retrieved context
- ask the chat model for a final answer
- return the answer along with the supporting `RateEmbedding` ids used in generation

16. Add tests for:

- embedding records are created
- similar questions retrieve relevant context
- rate updates still succeed when embedding generation is queued asynchronously

### Done When

- Embeddings are stored in PostgreSQL
- Natural-language questions can retrieve and summarize internal market data
- Core write flows do not depend on live AI latency

## Phase 10: Hardening And Delivery

### Tasks

1. Complete validator coverage for all input DTOs and confirm the global validation pipeline added in Phase 1 is enforcing them.

2. Add global exception handling middleware.

3. Return consistent `ProblemDetails` responses for all error codes:

- `400`
- `401`
- `403`
- `404`
- `409`
- `500`

4. Add ASP.NET Core rate limiting.

Required v1 policy:

- `30 requests per minute per IP`

5. Keep `HealthController` for smoke checks and add infrastructure readiness checks at `/healthz` for:

- PostgreSQL
- Redis

6. Add structured logging using the built-in ASP.NET Core logger with JSON console output unless a stronger sink requirement appears later.

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

- Confirm every controller uses `[ApiController]`.
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

## Resume Checklist For Another Chat

Whenever work pauses, update this file with:

- current phase status in the progress tracker
- the exact last successful commands
- the files or projects created since the previous checkpoint
- any fixed seed GUIDs that future work now depends on
- any verified external package or model decisions that were time-sensitive
- open blockers or environment issues

## Suggested Folder Layout

```text
backend/
  Vessel.sln
  global.json
  docker-compose.yml
  implementation_plan.md
  Vessel.Core/
    Entities/
    Enums/
    Exceptions/
  Vessel.Application/
    DTOs/
    Interfaces/
      Repositories/
      Queries/
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
    Mocks/
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
    Vessel.API.csproj
  Vessel.Tests/
    Unit/
    Integration/
    Mocks/
```
