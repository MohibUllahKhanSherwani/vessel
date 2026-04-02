# Product Requirements Document - Vessel MVP

**Version:** 1.1  
**Status:** In Development  
**Author:** Mohib Ullah Khan Sherwani  
**Last Updated:** April 2026

---

## 1. Product Summary

Vessel is an API-first marketplace for water tanker price discovery and booking in Pakistan. The MVP focuses on three cities: Karachi, Lahore, and Islamabad.

The problem is not lack of demand. The problem is market opacity. Consumers usually rely on informal networks, inconsistent pricing, and scattered providers. Vessel gives them one place to compare current rates, discover nearby providers, book reliably, and get notified when prices move.

For providers, the MVP offers a structured way to publish rates and manage booking requests. For admins, it provides a basic analytics layer to monitor activity and pricing trends.

---

## 2. Problem Statement

In major Pakistani cities, water tanker supply is a routine need for homes, apartments, businesses, and societies. Yet the market is still largely informal:

- prices vary sharply by area and season
- consumers have limited visibility into competing providers
- price history is not tracked in a usable way
- duplicate booking submissions are easy to trigger
- no practical public API exists for third-party tools or dashboards

Existing solutions are either single-provider apps, government portals, or low-trust products with limited coverage. None provide a true multi-provider marketplace with transparent pricing and a developer-friendly API.

---

## 3. MVP Goal

The MVP should prove that Vessel can act as a usable source of truth for tanker rates and bookings in a limited but real market scope.

By the end of the MVP, a user should be able to:

- register as a consumer and log in
- view areas and active provider rates
- discover providers near a target location
- submit a booking without duplicate creation on repeated requests
- create and manage price alerts
- receive real-time and job-driven alerting behavior
- ask natural-language questions about internal market data

By the end of the MVP, a provider should be able to:

- log in with a provisioned provider account
- publish and update area-specific rates
- view booking requests assigned to them
- confirm or cancel their own bookings

By the end of the MVP, an admin should be able to:

- access aggregate analytics
- view high-level booking and price trends
- access operational tools such as protected job dashboards

---

## 4. Target Users

| User Type | Primary Need |
|---|---|
| Consumer | Compare providers, book confidently, monitor price changes |
| Provider | Publish rates, manage incoming demand, maintain visibility |
| Admin | Monitor platform activity, pricing trends, and operations |
| Developer / Third Party | Read structured rate and availability data through the API |

---

## 5. Core MVP Features

### 5.1 Rate Intelligence

Consumers can browse active provider rates for a selected area without authentication.

MVP expectations:

- geographic model is `City -> Area`
- providers publish a price-per-gallon for each area they serve
- historical rate changes are preserved instead of overwritten
- consumers can view current rates and recent rate history

### 5.2 Provider Discovery

Consumers can find providers relevant to a location.

MVP expectations:

- discovery uses area coordinates and radius-based search
- results are sorted by distance first, then by price
- results represent active provider-area offerings, not generic provider directory listings

### 5.3 Booking Engine

Consumers can place bookings through the platform and providers can act on their own requests.

MVP expectations:

- only authenticated consumers can create bookings
- duplicate submissions are prevented with an idempotency key
- bookings capture a price snapshot so later rate changes do not rewrite old bookings
- providers can only update bookings assigned to them
- booking lifecycle is intentionally simple: `Pending`, `Confirmed`, `Cancelled`

### 5.4 Price Alerts

Consumers can define alert rules around pricing in an area.

MVP expectations:

- alerts are created for an area and target volume
- alert logic is based on total expected price for that chosen volume
- provider rate updates can trigger immediate real-time events
- scheduled jobs re-evaluate alerts on a recurring basis

### 5.5 Authentication And Access Control

The MVP uses role-based access control with three roles: Consumer, Provider, and Admin.

MVP expectations:

- public self-registration is available only for consumers
- provider and admin accounts are provisioned outside public signup
- authentication uses access tokens and refresh token rotation
- role boundaries are enforced across protected actions

### 5.6 Admin Analytics

Admins can inspect a small but useful set of platform metrics.

MVP expectations:

- top providers by confirmed booking activity
- average active prices by city
- booking volume trends over time
- price trends by area

### 5.7 AI Market Insights

The MVP includes a constrained AI feature focused on Vessel's own internal data.

MVP expectations:

- users can ask natural-language questions about internal market activity
- answers are grounded in stored Vessel data such as rate changes and alert events
- external news and broad web ingestion are not part of the MVP

---

## 6. User Experience Principles

The MVP should feel:

- transparent: users can see how rates differ across providers and areas
- dependable: booking submission should not create accidental duplicates
- fast to inspect: public rate lookup should work before a user commits to signup
- role-aware: each user type should see only the actions relevant to them
- extensible: the API should be usable by future frontend clients and third-party tools

---

## 7. Scope Boundaries

### In Scope For MVP

- Karachi, Lahore, and Islamabad
- multi-provider rate comparison by area
- provider discovery by location and radius
- consumer bookings with idempotency protection
- provider-side booking status updates
- consumer price alerts
- admin analytics
- authenticated AI Q&A over internal Vessel data
- documented REST API and responsive web client support

### Out Of Scope For MVP

- mobile apps
- in-app payments
- real-time driver tracking or dispatch logistics
- Urdu or multilingual support
- provider self-onboarding through public registration
- external news ingestion for AI answers
- advanced marketplace operations such as bulk procurement workflows

---

## 8. Success Criteria

The MVP is successful if it demonstrates that the product can support a coherent end-to-end marketplace flow.

Core success criteria:

- consumers can compare active rates across providers in supported cities
- a repeated booking request does not create duplicate bookings
- providers can update rates and manage only their own bookings
- price alerts can be created and can trigger correctly
- admins can retrieve meaningful aggregate metrics
- AI responses can reference internal market context rather than generic text generation

---

## 9. Non-Functional Product Expectations

These are product-level expectations, not implementation prescriptions.

- API behavior should be documented and testable before the frontend is complete
- access control must be reliable across Consumer, Provider, and Admin roles
- errors should be consistent enough for frontend and third-party clients to handle cleanly
- the system should support real-time price or alert event delivery where relevant
- the MVP should be deployable and operable in a normal modern web stack

---

## 10. Frontend Product Surfaces

The MVP web application should support four primary surfaces:

- Consumer experience: browse rates, discover providers, create bookings, manage alerts
- Provider experience: update rates, review booking requests, change booking status
- Admin experience: inspect analytics and system operations
- AI experience: ask product-specific market questions through a lightweight chat interface

The frontend should be responsive web, not mobile-native.

---

## 11. MVP Roadmap

The expected delivery order for the MVP is:

1. backend foundation and API flows
2. responsive web frontend
3. AI insight layer on internal data
4. post-MVP operational and expansion features

This order matters because the API and booking integrity rules are the product foundation.

---

## 12. Product Vision

If the MVP works, Vessel becomes more than a booking tool. It becomes a structured market layer for a fragmented, high-friction urban utility need.

The long-term opportunity includes:

- broader city coverage
- deeper provider participation
- stronger analytics and forecasting
- richer procurement workflows for societies and businesses
- ecosystem usage through the public API

The MVP does not need to solve the full market. It needs to prove that transparency, structured bookings, and internal market intelligence create real product value in this category.
