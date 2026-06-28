# HelloStack — Aspire + HotChocolate + EF Core + Relay

A minimal but **production-shaped** full-stack starter, and the companion code for the blog post
[**Zero to a production foundation in an afternoon**](https://michaeldubose.dev/blog/production-foundation-in-an-afternoon/).

It gets you to a styled **Hello, World!!** that is genuinely a foundation you could grow into a real
product:

- **.NET Aspire** orchestrates everything (Postgres, API, frontend, gateway) with one `dotnet run` and a
  dashboard with logs, traces, and metrics.
- A **layered HotChocolate GraphQL API** (Domain / Data / GraphQL) over **EF Core + PostgreSQL**, using
  **GreenDonut.Data** for cursor pagination, filtering, sorting, and projections.
- A **Vite + React + Relay** frontend, fronted by a **YARP** gateway so the browser and `/graphql` share
  a single origin (no CORS).
- **OpenTelemetry** wired up automatically via Aspire service defaults.

```
            ┌──────────────── Aspire AppHost ────────────────┐
 browser ─▶ │  YARP gateway ─▶ HotChocolate API ─▶ Postgres  │
            │       └────────▶ Vite/React/Relay              │
            └────────────────────────────────────────────────┘
   /graphql → API   ·   everything else → the React app   ·   single origin
```

> ✅ **Verified end to end** on .NET 10.0.300, Aspire 13.4.6, HotChocolate 16.3.0, GreenDonut.Data 16.3.0,
> Relay 21.0.1, Vite 8.1.0 — a headless browser through the gateway renders "Hello, World!!" served from
> Postgres.

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

Open the Aspire dashboard URL printed in the console, click the **gateway** endpoint, and you'll see the
styled **Hello, World!!** — its text pulled live from Postgres through GraphQL, with traces for the whole
request path in the dashboard.

## Regenerating the GraphQL schema & Relay types

The frontend's `web/schema.graphql` is committed so the project builds out of the box. If you change the
API's schema, regenerate it and recompile Relay:

```bash
# Export the schema from the API (needs a running Postgres; or just run the AppHost once)
dotnet run --project Api -- schema export --output web/schema.graphql

# Recompile Relay artifacts (also runs automatically on `npm run dev` / `npm run build`)
cd web && npm run relay
```

## Project layout

```
HelloStack.slnx
AppHost/            # Aspire orchestration: Postgres, API, Vite app, YARP gateway
ServiceDefaults/    # Aspire OpenTelemetry / health / resilience defaults
Api/
  Domain/           # Message entity (knows nothing about GraphQL or EF)
  Data/             # AppDbContext + MessageRepository (GreenDonut.Data paging)
  GraphQL/          # Query type
  Program.cs        # wires it all together
web/                # Vite + React + Relay frontend
  schema.graphql    # exported from the API; input to the Relay compiler
  relay.config.json
  vite.config.ts    # Relay babel transform + gateway-friendly allowedHosts
```

## Notes / gotchas this repo already handles

These are the non-obvious bits that make the bleeding-edge stack actually run — they're baked in so you
don't have to discover them:

- **`@vitejs/plugin-react` v6 dropped its `babel` option** (it transforms with oxc). Relay's `graphql\`\``
  tags are compiled via a dedicated `@rolldown/plugin-babel` pass in `vite.config.ts`.
- **Vite blocks proxied hosts by default** — `server.allowedHosts` includes `.aspire.dev.internal` so the
  dev server works behind the YARP gateway.
- **Relay codegen runs without Watchman** (`relay-compiler --noWatchman`) so no extra system dependency.
- **Cursor pagination needs a stable sort key** — the repository supplies a default sort + `Id` tiebreaker
  so `messages` works even when the client passes no `order`.
- The demo's `messages` query selects `text` (not the raw integer `id`) because Relay expects global
  string IDs; add HotChocolate global object identification if you want Relay-style node IDs.
