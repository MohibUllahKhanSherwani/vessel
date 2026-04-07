# Vessel | AI-Powered Water Tanker Marketplace

**Vessel** is an API-first platform for water tanker rate intelligence and booking management in Pakistan (Karachi, Lahore, and Islamabad). It replaces informal, fragmented pricing with a centralized, transparent marketplace.

---

### Key Features
- **Rate Intelligence:** Real-time price tracking with a **30-day historical audit log**.
- **Booking Engine:** Reliable booking lifecycle with **Redis-backed idempotency** to handle high-concurrency requests.
- **Provider Discovery:** Geolocation-based provider search implemented using a native **Haversine formula**.
- **AI-Powered Market Insights:** A **Semantic Kernel RAG pipeline** with **pgvector** to query market trends using natural language.
- **Real-Time Price Alerts:** Instant **SignalR** updates and **Hangfire**-orchestrated notification jobs for price threshold hits.

---

### 🛠 Technical Stack
| Layer | Technology |
|---|---|
| **Backend** | ASP.NET Core 10 (Clean Architecture / Controllers) |
| **Frontend** | React (Vite) + TypeScript + Tailwind CSS |
| **Database** | Supabase (PostgreSQL + EF Core 10) |
| **AI / RAG** | Semantic Kernel + Google Gemini (Free API) |
| **Caching** | Redis (Local or Supabase) |
| **Jobs** | Hangfire (recurring market analytics & alerts) |
| **Real-time** | SignalR (price broadcasts) |

---

### Architecture Overview
```text
backend/
  Vessel.API/            # Composition root, Controllers, Middleware, SignalR
  Vessel.Application/    # DTOs, Service Interfaces, Persistence Interfaces, Validators
  Vessel.Infrastructure/ # EF Core, Redis, Hangfire, Repositories, External Auth
  Vessel.Core/           # Domain Entities, Domain Exceptions, Enums
  Vessel.AI/             # Semantic Kernel RAG, Embeddings, Prompt Handling
  Vessel.Tests/          # xUnit Integration and Unit tests
```

---

### Documentation
- [**Full Product Requirements Document (PRD)**](vessel_prd.md)
- [**Backend Phase-by-Phase Implementation Roadmap**](backend/phasebyphase_implementation_plan.md)

### Current Progress
- ✅ **Phases 1-4 Complete**: Architecture, Database Seeding, Authentication Middleware, and Rate Intelligence. (Done)
- 🚀 **Next Run**: Phase 5: Provider Discovery (Search/Haversine formula implementation).

---

### Getting Started
**Local Services (PostgreSQL + Redis + pgvector):**
```bash
docker compose up -d
```
**Run the API:**
```bash
dotnet run --project backend/Vessel.API
```
*Swagger UI will be available at `/swagger` once launched.*

---
*Created by [Mohib Ullah Khan Sherwani](https://github.com/MohibUllahKhanSherwani) | 2026*
