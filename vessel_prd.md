# Product Requirements Document — Vessel

**Version:** 1.0  
**Status:** In Development  
**Author:** Mohib Ullah Khan Sherwani
**Last Updated:** March 2026

---

## 1. Problem Statement

Pakistan faces a chronic municipal water shortage. In major cities like Karachi, Lahore, and Islamabad, a significant portion of residential and commercial water needs are fulfilled by privately-operated water tankers. This is not a niche use case — Karachi alone sees an estimated 8,000–10,000 tanker deliveries per day.

Despite this scale, the market operates almost entirely informally. Consumers find providers through word of mouth, WhatsApp forwards, or phone directories. Prices are set arbitrarily and shift with fuel costs, seasonality, and demand spikes — sometimes doubling within a single year with no warning. There is no systematic way for a consumer to compare providers, track price trends, or get notified when rates change in their area.

---

## 2. Existing Solutions and Their Failures

Four solutions exist in this space. None of them solve the core problem.

### KWSB Online Tanker System (Karachi)
A government-operated mobile app for requesting tankers through KWSB hydrants. Structurally limited: it is a single-supplier system tied to one city's government infrastructure. In practice, the app is broken — the login screen fails, address submission hangs, and user reviews average under 2 stars with no updates since its last known patch cycle. It is a government portal, not a marketplace.

### Waterlink (waterlink.pk)
A Karachi-based operation that guarantees lower prices than the open market. The key detail: Waterlink *is* the provider. They manage their own fleet and their app is just an order form. There is no comparison between competitors, no rate visibility, and no data layer. Outside Karachi, it does not exist.

### TankerWala.pk
The most polished of the existing attempts. A mobile app that lets users select tanker size, drop a pin, and schedule delivery. On inspection: 2.0 stars, 12 user reviews, 1,000+ downloads, last updated May 2023, developer contact is a personal Gmail address. More critically, TankerWala is itself a single tanker company with an app — not a marketplace. There is no price comparison, no competing providers listed, no coverage outside Karachi. Three of the top reviews call it broken.

### The Gap None of Them Address
- No solution covers Lahore or Islamabad at all
- No solution shows prices from multiple competing providers side by side
- No solution tracks price history or lets consumers set rate-change alerts
- No solution exposes a public API that developers or civic tools can build on
- No solution applies any fraud-prevention pattern (e.g. idempotency) to bookings

---

## 3. What Vessel Does

Vessel is a web-based, API-first platform for water tanker rate intelligence and booking management across Pakistani cities.

The core product has three layers:

**Rate Intelligence** — Providers publish their price-per-gallon for specific geographic areas. Every rate change is stored, not overwritten. Consumers can view current rates across multiple providers for their area, see a 30-day price history, and subscribe to threshold alerts (e.g., "notify me when any provider in DHA Phase 6 exceeds Rs 4,500 for 1,000 gallons").

**Booking Engine** — Consumers book directly through the platform. Bookings are protected by an idempotency key to prevent duplicate submissions. Providers manage their own confirmed/cancelled status. Role-based access ensures providers can only modify their own listings.

**Open API** — Every feature is exposed through a documented REST API. Rate data, provider listings, booking flows, and alert subscriptions are all accessible programmatically. This makes Vessel a foundation that civic dashboards, property management tools, or third-party apps can build on — something no existing solution offers.

---

## 4. Target Users

| User Type | What They Need |
|---|---|
| **Consumer (Resident / Business)** | Find the cheapest provider in their area, book a delivery, get alerted to price spikes |
| **Provider (Tanker Company)** | Publish and update their rates, manage incoming bookings, track their booking history |
| **Admin** | Oversee platform activity, view aggregate analytics, manage users and providers |
| **Developer / Third Party** | Access rate and availability data via API to build on top of the platform |

---

## 5. Core Features

### 5.1 Area and Rate Management
- Geographic coverage structured as: City → Area (e.g., Islamabad → F-10 Markaz)
- Providers set price-per-gallon per area
- Rate changes create a new history record — old rates are never deleted
- Public endpoint for current rates requires no authentication

### 5.2 Provider Discovery
- Search providers by area
- Geolocation-based search: find providers within a specified radius using the Haversine formula
- Results sorted by distance, then by price
- No third-party geo libraries — algorithm implemented from scratch

### 5.3 Booking Engine
- Authenticated consumers book a provider for a specific area and volume
- Idempotency key on every booking request — duplicate submissions return the existing booking
- Booking status lifecycle: `Pending → Confirmed → Cancelled`
- Providers can only confirm or cancel bookings assigned to them

### 5.4 Price Alerts
- Consumers subscribe to price thresholds for an area
- Background job runs daily, compares current rates against subscriptions, queues notifications
- Real-time push event when a provider updates their rate (delivered via SignalR to connected clients)

### 5.5 Analytics (Admin)
- Top 5 most-booked providers
- Average price per city
- Booking volume per day over the last 30 days
- Price trend data per area over configurable time windows

### 5.6 Authentication and Access Control
- JWT-based authentication with refresh token rotation
- Three roles: Consumer, Provider, Admin
- Resource-based authorization: Providers can only edit their own rates
- Rate limiting: 30 requests/minute per IP

### 5.7 AI-Powered Market Insights (RAG)
- Users can naturally ask questions like "What is the cheapest time of year to book a tanker in DHA?" or "Why did prices spike last month?"
- RAG (Retrieval-Augmented Generation) synthesizes historical price data, localized news (e.g., KWSB pipeline issues), and provider updates to provide natural language answers.
- Implemented securely with vector embeddings of historical rate changes and system alerts.

---

## 6. Out of Scope (v1)

The following are explicitly excluded from the initial version:

- Mobile application (responsive web only)
- Payment processing or in-app transactions
- Real-time driver tracking or GPS dispatch
- Multi-language support (Urdu interface)

---

## 7. Technical Stack

| Layer | Technology |
|---|---|
| Frontend Web App | React (Vite) + TypeScript + Tailwind CSS |
| Backend Framework | ASP.NET Core 10 (Controllers) |
| AI / RAG Engine | Semantic Kernel + OpenAI |
| Database | PostgreSQL via EF Core (code-first migrations) |
| Vector Store | pgvector (for RAG embeddings) |
| Caching | Redis |
| Real-time | SignalR |
| Containerization | Docker + Docker Compose |
| CI/CD | GitHub Actions |

---

## 8. Architecture Overview

```text
vessel-web/             ← React Frontend, Tailwind, Axios, React Query
Vessel.API/             ← Controllers, Middleware, DI, SignalR, Swagger, Program.cs
Vessel.Core/            ← Domain Entities, Enums, Domain Exceptions
Vessel.Application/     ← DTOs, Validators, Service Interfaces, Persistence Interfaces
Vessel.AI/              ← Semantic Kernel RAG Pipeline, Embeddings, Prompt Handling
Vessel.Infrastructure/  ← EF Core, Repositories, Redis, pgvector, Hangfire, Auth
Vessel.Tests/           ← xUnit unit and integration tests
```

The system runs as containerized Docker services encompassing the React Frontend, Backend API, PostgreSQL (with pgvector), and Redis. All secrets and connection strings are injected via environment variables.

---

## 9. Web Application Features (Frontend)

The frontend is a responsive Single Page Application (SPA) built with React and Tailwind CSS, focused on delivering a seamless experience across devices.

- **Consumer Dashboard:** Map-based interface for selecting areas, comparing prices using interactive charts, and booking deliveries. Features real-time price alert configuration.
- **Provider Portal:** A dedicated workspace for tanker companies to update their per-gallon rates in bulk, view incoming booking requests, and manage fulfillment status.
- **Admin Console:** High-level metrics dashboard showing market liquidity, price trends across cities, and system health.
- **AI Chat Assistant:** A floating widget allowing users to query market trends using natural language, powered by the backend RAG pipeline.

---

## 10. Product Roadmap & Future Horizons

The project implementation is structured sequentially, focusing entirely on the backend foundation before expanding to the frontend and AI capabilities:

- **Phase 1: Backend Foundation (Upcoming)** - Developing the core API, database schema, rate tracking, and booking abstraction using ASP.NET Core and PostgreSQL.
- **Phase 2: Web Frontend** - Building the responsive React/Vite SPA, integrating with the backend API to deliver the Consumer Dashboard and Provider Portal.
- **Phase 3: AI & Market Insights (RAG)** - Integrating the RAG pipeline with vector databases to generate natural language explanations for price volatility and market trends.
- **Phase 4: Advanced Operations & B2B Expansion** - Partnering with large residential societies for automated bulk procurement and consolidated billing.

---

## 11. Market Realities & Vision

Vessel is designed as a fully functional, production-ready product aimed at solving a deeply entrenched, informal market problem in Pakistan. The goal is to build a robust technological foundation capable of handling real-world complexity without relying on "toy project" constraints.

While the technical architecture (backend, frontend, AI integrations) maps exactly to a real startup's needs, tackling the operational realities of the local water tanker scene—such as onboarding informal operators and enforcing transparent pricing models against the "tanker mafia"—presents significant offline challenges. Even if these real-world market friction points stall immediate commercial adoption, Vessel serves as an authentic system modeling a highly fragmented, dynamic market at scale.