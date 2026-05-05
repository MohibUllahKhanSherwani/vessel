# Supabase Cloud Setup

Use Supabase Cloud as the normal development database. The local `postgres-test`
container is only for isolated test work.

## 1. Copy Your Supabase Connection String

In Supabase Dashboard, open your project and click **Connect**.

For this ASP.NET backend:

- Prefer **Direct connection** if your network supports IPv6.
- Use **Session pooler** if you need IPv4.
- Avoid **Transaction pooler** for this app because EF Core and Hangfire are
  long-running backend clients.

Use an Npgsql-style connection string:

```text
Host=<host>;Port=5432;Database=postgres;Username=<username>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
```

Session pooler usernames usually look like:

```text
postgres.<project-ref>
```

Important:

- Copy the **exact** Session pooler host from Supabase Dashboard -> **Connect**.
  Do not guess or hand-build `aws-<n>-<region>.pooler.supabase.com`.
- Replace the password placeholder with the project's **database password**,
  not the anon key, service role key, or any API key.

## 2. Store Secrets Locally

Run these from the repository root:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<host>;Port=5432;Database=postgres;Username=<username>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true" --project backend/Vessel.API
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379" --project backend/Vessel.API
dotnet user-secrets set "Jwt:Key" "<at-least-32-character-development-secret>" --project backend/Vessel.API
dotnet user-secrets set "Gemini:ApiKey" "<your-gemini-api-key>" --project backend/Vessel.API
```

Do not put the real Supabase password in `appsettings.json`.

## 3. Start Redis Only

The app still uses Redis for booking idempotency.

```powershell
docker compose -f backend/docker-compose.yml up -d redis
```

## 4. Run the API

In Development, the API applies EF Core migrations on startup.

```powershell
dotnet run --project backend/Vessel.API
```

Open Swagger at the URL printed by `dotnet run`, usually:

```text
https://localhost:7235/swagger
```

## Notes

- Supabase must have the `vector` extension available. The EF model requests it
  through migrations for pgvector-backed AI embeddings.
- Hangfire uses the same Supabase database and creates its own `hangfire`
  schema.
- Keep local `postgres-test` for tests if you later add integration tests that
  should not touch Supabase.

## Troubleshooting

- `The requested name is valid, but no data of the requested type was found`
  usually means you used the direct `db.<project-ref>.supabase.co` host on an
  IPv4-only network. Switch to the exact **Session pooler** string from
  Supabase Connect.
- `28P01: password authentication failed for user "postgres"` usually means
  one of two things:
  - the Session pooler host was guessed instead of copied from Supabase Connect
  - the database password is wrong and should be reset in Supabase Database
    Settings before updating `dotnet user-secrets`
- Supabase pooler/auth errors can still mention `postgres` even when your
  username is the Session pooler form `postgres.<project-ref>`.
