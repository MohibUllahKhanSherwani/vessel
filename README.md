# Vessel | AI-Powered Water Tanker Marketplace (Local Demo Mode)

**Vessel** is an API-first platform for water tanker rate intelligence and booking management in Pakistan (Karachi, Lahore, and Islamabad). It replaces informal, fragmented pricing with a centralized, transparent marketplace.

---

### ⚡ Quick Start (No Docker, No Database Setup Required!)

This system has been converted to **Pure Local Bare-Metal Mode**. You can run and show this project to anyone immediately without installing PostgreSQL, Redis, or Docker!

#### 1. Run the Backend API:
```bash
cd backend/Vessel.API
dotnet run
```
* **Swagger Documentation & Testing UI:** [http://localhost:5000/swagger](http://localhost:5000/swagger) (or `https://localhost:5001/swagger`)
* **Database:** Automatically initializes in-memory EF Core database on startup and seeds full demo data for Karachi, Lahore, and Islamabad.

#### 2. Run the Frontend (if applicable):
```bash
cd frontend
npm install
npm run dev
```

---

### Key Features
- **Rate Intelligence:** Real-time price tracking with historical audit logs.
- **Booking Engine:** Reliable booking lifecycle with in-process idempotency cache.
- **Provider Discovery:** Geolocation-based provider search using native Haversine formula.
- **AI Market Insights:** Semantic Kernel RAG pipeline with in-memory Cosine Similarity.
- **Real-Time Price Alerts:** Instant SignalR updates powered by a lightweight `IHostedService` background job.

---

### 🛠 Technical Stack (Standalone Local Mode)
| Layer | Technology |
|---|---|
| **Backend** | ASP.NET Core 10 (Clean Architecture / Controllers) |
| **Database** | EF Core 10 In-Memory DB (Auto-seeded with realistic data) |
| **Caching** | ConcurrentDictionary In-Memory Cache (No Redis needed) |
| **Background Jobs** | ASP.NET Core `IHostedService` (No Hangfire needed) |
| **Real-time** | SignalR (Price broadcasts) |
| **AI / RAG** | Semantic Kernel + Google Gemini (Optional) |

---

### 🔑 Pre-seeded Demo Credentials
Upon running `dotnet run`, the following accounts are automatically created for demoing:

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@vessel.com` | `Admin123!` |
| **Provider** | `provider@vessel.com` | `Provider123!` |
| **Consumer** | `consumer@vessel.com` | `Consumer123!` |

---

*Created by [Mohib Ullah Khan Sherwani](https://github.com/MohibUllahKhanSherwani) | 2026*
