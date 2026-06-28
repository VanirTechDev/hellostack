# HelloStack — Aspire + HotChocolate + EF Core + Relay

A small but **production-shaped** full-stack starter, and the companion code for the blog post
[**Zero to a production foundation in an afternoon**](https://michaeldubose.dev/blog/production-foundation-in-an-afternoon/).

It's a browsable library of **The Wheel of Time** that actually exercises the hard parts — cursor
**pagination**, **filtering**, and **sorting** — over a stack you could grow into a real product:

- **.NET Aspire** orchestrates everything (Postgres, API, frontend, gateway) with one `dotnet run` and a
  dashboard with logs, traces, and metrics.
- A **layered HotChocolate GraphQL API** (Domain / Data / GraphQL) over **EF Core + PostgreSQL**, using
  **GreenDonut.Data** for cursor pagination, filtering, sorting, and projections.
- **Proper Relay nodes** — `Book` implements the `Node` interface with globally-unique opaque IDs and a
  `node(id:)` resolver (HotChocolate global object identification).
- A **Vite + React + Relay** frontend (`usePaginationFragment` for "load more", `refetch` for sort/filter)
  fronted by a **YARP** gateway so the browser and `/graphql` share a single origin (no CORS).
- **OpenTelemetry** wired up automatically via Aspire service defaults.

```
            ┌──────────────── Aspire AppHost ────────────────┐
 browser ─▶ │  YARP gateway ─▶ HotChocolate API ─▶ Postgres  │
            │       └────────▶ Vite/React/Relay              │
            └────────────────────────────────────────────────┘
   /graphql → API   ·   everything else → the React app   ·   single origin
```

> ✅ **Verified end to end** on .NET 10.0.300, Aspire 13.4.6, HotChocolate 16.3.0, GreenDonut.Data 16.3.0,
> Relay 21.0.1, Vite 8.1.0 — a headless browser through the gateway renders the paginated library, and the
> sort / filter / load-more operations are confirmed against the live API.

## Prerequisites

- **.NET 10 SDK**
- **Node 20+**
- A container runtime: **Docker or Podman** (Aspire pulls the Postgres and YARP images for you)

## Run it

```bash
# 1. Install frontend deps once (also generates Relay artifacts via the predev/prebuild hook)
cd web && npm install && cd ..

# 2. Start everything through Aspire
dotnet run --project AppHost
```

Open the Aspire dashboard URL printed in the console, click the **gateway** endpoint, and you'll get the
library: 15 Wheel of Time books seeded into Postgres, served through a paginated GraphQL connection. Sort
by series / year / length / title, filter by author, search by title, and **Load more** to page through —
each one a real GraphQL round-trip, with traces for the whole path in the dashboard.

## Regenerating the GraphQL schema & Relay types

The frontend's `web/schema.graphql` is committed so the project builds out of the box. If you change the
API's schema, regenerate it and recompile Relay:

```bash
# Export the schema (run from the Api folder so the relative path lands in web/)
cd Api && dotnet run -- schema export --output ../web/schema.graphql && cd ..

# Recompile Relay artifacts (also runs automatically on `npm run dev` / `npm run build`)
cd web && npm run relay
```

## Project layout

```
HelloStack.slnx
AppHost/            # Aspire orchestration: Postgres, API, Vite app, YARP gateway
ServiceDefaults/    # Aspire OpenTelemetry / health / resilience defaults
Api/
  Domain/           # Book entity (knows nothing about GraphQL or EF)
  Data/             # AppDbContext + BookRepository (GreenDonut.Data paging)
  GraphQL/
    Query.cs        # the `books` connection (paging + filtering + sorting)
    BookNode.cs     # [ObjectType<Book>] + [NodeResolver] → Relay node + global id
  Program.cs        # wires it together; seeds the Wheel of Time series
web/                # Vite + React + Relay frontend
  src/App.tsx       # paginated, filterable, sortable library (usePaginationFragment)
  src/BookCard.tsx  # colocated Relay fragment component
  schema.graphql    # exported from the API; input to the Relay compiler
  vite.config.ts    # Relay babel transform + gateway-friendly allowedHosts
```

## Notes / gotchas this repo already handles

The non-obvious, version-specific bits that make this stack actually run — baked in so you don't have to
rediscover them:

- **`@vitejs/plugin-react` v6 dropped its `babel` option** (it transforms with oxc). Relay's `graphql\`\``
  tags are compiled via a dedicated `@rolldown/plugin-babel` pass in `vite.config.ts`.
- **HotChocolate 16 ships cost analysis on** with a default max field cost of 1000 — a paged connection
  plus filtering and sorting exceeds it, so `Program.cs` raises the ceiling with `ModifyCostOptions`.
- **Vite blocks proxied hosts by default** — `server.allowedHosts` includes `.aspire.dev.internal` so the
  dev server works behind the YARP gateway.
- **Relay codegen runs without Watchman** (`relay-compiler --noWatchman`) so no extra system dependency.
- **Cursor pagination needs a stable sort key** — `BookRepository` supplies a default sort + `Id`
  tiebreaker so `books` works even when the client passes no `order`.
- **Relay needs globally-unique string IDs** — `Book` is a Relay node via `[NodeResolver]` +
  `AddGlobalObjectIdentification()`, so its `id` is an opaque global ID the client can refetch.
