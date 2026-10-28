# Plan: Convert Backend from Python/FastAPI to C#/.NET

**Date:** 16/08/2026
**Status:** In Progress (Phase 7 complete — Docker not smoke-tested, Docker Desktop unavailable in this environment)

## Objective

Replace the Python/FastAPI backend (`Server/`) with a C#/.NET backend, using the local
`C:\Temp\GitHub\FlowingDefault` repository as an architectural template, and restructure the
repository so all buildable source code (backend and frontend) lives under a new `src/` folder.

Concrete gains:

- Aligns the backend with the user's preferred stack (.NET 8+/ASP.NET Core, EF Core, xUnit,
  Serilog — per global `CLAUDE.md`) instead of the current one-off Python/FastAPI choice.
- Fixes several latent bugs identified in the current implementation along the way (see
  "Known bugs fixed during the port" below) instead of carrying them forward.
- Adds typed request/response contracts throughout (Pydantic's dynamic validation becomes
  compile-time-checked C# DTOs and records).
- Replaces the Python app's "ask the model to emit a JSON command array as text, then
  string-parse it" mechanism with Semantic Kernel's native tool/function calling — commands
  become typed, independently unit-testable functions instead of a fragile text contract (see
  Decision 6).
- Adds .NET Aspire for local orchestration (API + Postgres, and optionally the web frontend),
  replacing today's "run uvicorn and npm run dev in two terminals" workflow.
- Establishes a `src/` root so the repository separates source from project meta (docs, Docker
  compose, build scripts) — a structure this repo does not currently have.

## Current State

### Backend (`Server/`)

- FastAPI app in `Server/api.py`: CORS is wide open (`allow_origins=["*"]`), DB access is a
  SQLAlchemy `Session` injected via `Depends(get_db)`, there is no authentication.
- Routes: `GET /health`, `GET /items`, `POST /items`, `DELETE /items/{id}`,
  `PATCH /items/{id}/checked`, `POST /chat` (streaming).
- `Server/llm.py`: an `LLMClient` ABC with `stream_chat(messages)`, two implementations
  (`ChatGPTClient` wrapping `openai.AsyncOpenAI`, `OllamaClient` wrapping `ollama.AsyncClient`),
  and a `create_llm_client(llm_type, model, api_key)` factory. A module-level singleton is built
  once at import time from `LLM`/`MODEL`/`OPENAI_API_KEY` env vars read directly via
  `os.getenv`. `get_response(messages)` injects a `SYSTEM_PROMPT` at index 0 and swallows
  exceptions into a yielded `"Error: ..."` string instead of raising.
- The `/chat` endpoint reads `{"messages":[...]}` or the legacy `{"message": "..."}` shape,
  streams LLM tokens straight to the client while accumulating the full text, and — only after
  the stream ends — strips markdown fences, JSON-parses a trailing command array
  (`[{"command": "AddItem"|"RemoveItem"|"CheckItem"|"UncheckItem", "value": "..."}]`), and
  executes DB side effects using the same request-scoped `Session` (`RemoveItem`/`CheckItem`/
  `UncheckItem` match via case-insensitive substring `ilike`, first match only).
- `Server/Models/item.py`: SQLAlchemy `Item` (`id`, `description varchar(255) not null`,
  `checked bool default false`, `created_at`/`updated_at` server-side timestamps).
  `Server/Models/schemas.py`: Pydantic `ItemCreate`, `ItemResponse` (`from_attributes`),
  `ItemCheckedUpdate`, plus an unused `ItemUpdate`.
- `Server/Models/database.py`: `DATABASE_URL` env var (required, raises if unset — no code
  default despite docs claiming one), `get_db()` generator dependency, `init_db()` =
  `Base.metadata.create_all()`. No Alembic — no migration history exists to port.
- No settings/config class anywhere: every module calls `os.getenv` directly, and
  `load_dotenv()` is called redundantly in both `api.py` and `llm.py`.
- `Server/logger.py` is a hand-rolled file logger. `Server/main.py` /
  `Server/service_manager.py` wrap the app with a Linux-systemd service installer — a
  process-management concern that has no .NET equivalent to port (superseded by standard
  container/host logging and orchestration).
- Tests: 151 pytest tests. `test_items_api.py`, `test_chat_api.py`, `test_llm.py`,
  `test_models.py`, `test_schemas.py` are all portable/relevant. `test_logger.py` and
  `test_service_manager.py` exercise the Linux-only concerns above and are **not** ported.
- `Server/Dockerfile`: single-stage `python:3.14-slim`, `pip install .`, runs `main.py`,
  `EXPOSE` via `API_PORT` build arg (default 5002), curl-based `HEALTHCHECK` against `/health`.

### Known bugs fixed during the port (not blindly carried forward)

- `PATCH /items/{id}/checked` returns a raw `{"message": "Item not found"}` dict against a
  typed `response_model=ItemResponse` when the item is missing — a latent FastAPI validation
  error. The .NET port returns a proper `404 Not Found`.
- `/chat`'s `StreamingResponse` declares `media_type="text/plain"` while sending SSE-shaped
  headers — inconsistent. The .NET port uses `text/event-stream` consistently.
- Command-execution/JSON-parse errors in `/chat` are caught and only `print()`-ed, never
  surfaced to the client, even though the HTTP response has already completed with `200 OK`.
  The .NET port fixes this as a side effect of adopting Semantic Kernel tool-calling
  (Decision 6): plugin functions return a descriptive result string (e.g. "no item found
  matching 'eggs'") that the model narrates back to the user in its normal streamed reply,
  instead of failing silently — see Design Decision 5.
- The unused `ItemUpdate` Pydantic schema is dropped; only the DTOs that map to real endpoints
  are ported.

### Reference repository (`C:\Temp\GitHub\FlowingDefault`)

- All .NET source sits under `Backend/` (a flat sibling of `Frontend/`, not under `src/` — this
  repo's `src/` requirement is a deviation from the template, introduced per the user's explicit
  request, not something copied from it).
- Four projects in one `.sln`, referenced as `Api → Core`, `Console → Core`, `Tests → Core`:
  - `FlowingDefault.Api` (`Microsoft.NET.Sdk.Web`): Controllers, DTOs, Middleware, `Program.cs`.
  - `FlowingDefault.Core` (plain class library, bottom of the graph): domain models as C#
    `record` types, EF Core `DbContext`, Fluent-API entity configurations under
    `Configurations/`, `Migrations/`, business-logic `Services/`.
  - `FlowingDefault.Console` (Exe stub, has `EFCore.Tools`): host for design-time `dotnet ef`
    commands.
  - `FlowingDefault.Tests` (references `Core` only, not `Api`): MSTest, one project.
- `FlowingDefaultDbContext`: two constructors (parameterless → Sqlite dev fallback;
  `DbContextOptions<T>` → whatever Program.cs configured, e.g. Postgres).
  `OnConfiguring` only sets Sqlite `if (!optionsBuilder.IsConfigured)`. `OnModelCreating` uses
  `ApplyConfigurationsFromAssembly`. `EnsureDatabaseAndAdminUser()` calls `Database.Migrate()` +
  seeds, invoked once from `Program.cs` before `app.Run()`.
- **Cross-database migration pattern** (`Backend/FlowingDefault.Core/Migrations/ProviderTypes/`)
  — the one directly reusable abstraction pattern in this template:
  `IMigrationProviderTypes` (interface, e.g. `AsAutoIncrementPrimaryKey(...)`), one
  implementation per provider (`SqliteMigrationProviderTypes`,
  `PostgresMigrationProviderTypes`), and a static `MigrationProviderTypes.For(string
  activeProvider)` factory backed by a `Dictionary<string, IMigrationProviderTypes>` keyed by
  EF Core's provider invariant name. Each migration's `Up()` resolves the right implementation
  via `migrationBuilder.ActiveProvider`, so one migration file targets both Sqlite and Postgres.
- `DatabaseProvider` enum (`Sqlite`/`Postgres`) parsed from `DATABASE_PROVIDER` in `Program.cs`;
  Postgres calls `UseNpgsql(CONNECTION_STRING)`; Sqlite (default) is left unconfigured so the
  `DbContext`'s own fallback applies.
- `Program.cs`: `DotNetEnv.Env.TraversePath().Load()` first, then Serilog (file sink),
  OpenTelemetry via a `Telemetry.cs` extension (OTLP exporter gated on
  `OTEL_EXPORTER_OTLP_ENDPOINT`), JWT auth (not applicable here — the Python app has no auth),
  one permissive CORS policy, Swagger gated to `Development`, reflection-based
  `AddInfrastructuralServices()` that scans `*.Core.Services` and registers every concrete class
  as Scoped, DbContext registration branching on `DATABASE_PROVIDER`.
- No `appsettings.json` is checked in — configuration is entirely `.env` + `DotNetEnv`, read via
  `Environment.GetEnvironmentVariable` directly (no `IOptions<T>` binding).
- No Docker/Dockerfile/compose in the template at all.
- No streaming/SSE pattern anywhere in the template.
- Testing: MSTest, EF Core **InMemory** provider (not Sqlite) via a `TestDbContext` mock.

## Proposal

### Repository layout — before / after

```
GroceryListAi/                          GroceryListAi/
├── Server/  (Python)                   ├── src/
├── Web/                                │   ├── Server/  (.NET solution)
├── docs/                               │   ├── Web/
├── docker-compose.yml                  │   └── Legacy/  (old Python Server/, kept for reference)
├── README.md, CLAUDE.md                ├── docs/
├── build-web-*.sh/.bat                 │   └── prototype/  (moved from root — see Decision 7)
├── prototype/                          ├── docker-compose.yml
└── .env, .env.example                  ├── README.md, CLAUDE.md
                                         ├── build-web-*.sh/.bat  (paths updated)
                                         └── .env, .env.example
```

`docs/`, `docker-compose.yml`, `README.md`, `CLAUDE.md`, `.env`/`.env.example`, and the build
scripts stay at the repo root — they are project metadata/tooling, not buildable source.
`prototype/` moves under `docs/` rather than `src/` (Decision 7) since it isn't buildable source
either.

`src/Legacy/` holds the retired Python backend, moved there verbatim (not deleted) once
`src/Server/` reaches feature parity — see Phase 7. It is not referenced by any build, test, or
deployment step; it exists purely as an easy-to-browse reference during and after the migration,
without relying on `git log`/checking out an old commit to see how something used to work.

### .NET solution layout (`src/Server/`)

```
src/Server/
├── GroceryListAi.sln
├── GroceryListAi.AppHost/          # .NET Aspire orchestrator (local dev only)
├── GroceryListAi.ServiceDefaults/  # Aspire shared defaults (OpenTelemetry, health checks, resilience)
├── GroceryListAi.Api/              # Microsoft.NET.Sdk.Web — Controllers, Program.cs
│   └── Controllers/
│       ├── ItemsController.cs
│       └── ChatController.cs
├── GroceryListAi.Core/             # domain, EF Core, LLM abstraction, business services
│   ├── Models/Item.cs
│   ├── Configurations/ItemConfiguration.cs
│   ├── GroceryListAiDbContext.cs
│   ├── Migrations/
│   │   └── ProviderTypes/          # ported from FlowingDefault (see Decision 2)
│   ├── Llm/
│   │   ├── KernelFactory.cs        # builds a Kernel with the right SK connector for LLM env var
│   │   └── GroceryListPlugin.cs    # KernelFunctions: AddItem/RemoveItem/CheckItem/UncheckItem
│   └── Services/
│       └── ItemService.cs
└── GroceryListAi.Tests/            # xUnit
```

Reference graph mirrors the template: `Api → Core`, `AppHost → Api` (as an Aspire project
reference for orchestration), `Tests → Core` (+ `Api` for `WebApplicationFactory` integration
tests, since the port needs HTTP-level coverage of streaming that the template's `Core`-only
test project doesn't need).

### API contract — same shape as today, fixed where noted

| Method | Path | Change from Python |
|---|---|---|
| GET | `/health` | none |
| GET | `/items` | none |
| POST | `/items` | none |
| DELETE | `/items/{id}` | none (still 200 either way, matching current REST-light style) |
| PATCH | `/items/{id}/checked` | returns `404` on not-found instead of a mistyped 200 body |
| POST | `/chat` | `Content-Type: text/event-stream` (was `text/plain`) |

### Config — env vars

| Var | Today (Python) | Proposed (.NET) |
|---|---|---|
| `LLM` | `chatgpt` \| `ollama` | unchanged |
| `MODEL` | required | unchanged |
| `OPENAI_API_KEY` | required if `LLM=chatgpt` | unchanged |
| `API_PORT` | used by `main.py` | unchanged (Kestrel port) |
| `CORS_ORIGINS` | documented, not code-enforced | bound and enforced |
| `DATABASE_URL` | single URL, dialect-sniffed | **replaced** by `DATABASE_PROVIDER` + `CONNECTION_STRING` — see Decision 1 |

## Design Decisions

### 1. Replace `DATABASE_URL` with `DATABASE_PROVIDER` + `CONNECTION_STRING`

EF Core has no equivalent of SQLAlchemy's single-URL dialect sniffing — each provider
(`UseSqlite`, `UseNpgsql`) takes its own connection-string shape and is chosen in code, not
inferred from a URL scheme. Splitting into an explicit `DATABASE_PROVIDER` enum
(`Sqlite`/`Postgres`) plus a provider-appropriate `CONNECTION_STRING` matches exactly how
FlowingDefault already does it, and avoids writing a bespoke URL parser just to reconstruct
what the enum gives for free.

*Alternative considered and rejected:* keep a single `DATABASE_URL` and parse the scheme
(`sqlite:///`, `postgresql://`) in C# to pick a provider — rejected as unnecessary parsing code
that duplicates what an explicit env var already states directly, and it would be the only
place in the .NET codebase doing URL-scheme sniffing.

**Decided with the user** — confirmed to proceed with the split, accepting that every
developer's local `.env` needs a one-time manual update when Phase 1 lands.

### 2. Port FlowingDefault's cross-provider migration pattern verbatim

`IMigrationProviderTypes` + `SqliteMigrationProviderTypes` + `PostgresMigrationProviderTypes` +
`MigrationProviderTypes.For(...)` is copied into `GroceryListAi.Core/Migrations/ProviderTypes/`
unchanged in structure. It is the only way to keep a single migration file working against both
the Sqlite dev/test database and the Postgres production database for the auto-increment `Id`
column, and the template already solves this problem correctly.

*Alternative considered and rejected:* maintain separate migration folders per provider —
rejected because EF Core's migration history is provider-agnostic by design once
`ActiveProvider`-branching is used, and duplicating history folders would double the
migration-maintenance burden for a one-entity schema.

### 3. Testing: xUnit + SQLite, not MSTest + EF InMemory

The user's global and project `CLAUDE.md` both specify xUnit for .NET backend testing, and the
`Flowing-EFCore` skill's stated convention is SQLite for integration tests / Postgres for
production. Both override FlowingDefault's own choice (MSTest + EF Core InMemory provider) for
this project.

*Alternative considered and rejected:* copy FlowingDefault's MSTest + InMemory setup as-is for
consistency with the reference repo — rejected because it directly contradicts explicit,
current standing instructions for this user's projects, which take precedence over one
reference repo's historical choice.

### 4. `.NET Aspire` for local dev orchestration only — not a deployment mechanism

`GroceryListAi.AppHost` wires up the `Api` project against a Postgres container
(`Aspire.Hosting.PostgreSQL`) and, optionally, the `Web` frontend via
`Aspire.Hosting.NodeJs`, so `dotnet run --project src/Server/GroceryListAi.AppHost` replaces
today's two-terminal `uvicorn` + `npm run dev` workflow. Production deployment continues to be
`docker-compose.yml` at the repo root (backend on port 5002, frontend on port 82, per the
existing Deployment Notes in `CLAUDE.md`) — Aspire's manifest/publish path is not adopted here,
to avoid changing the deployment story as part of a backend-language migration.

*Alternative considered and rejected:* skip Aspire and just document
`dotnet run --project GroceryListAi.Api` + a local Postgres — rejected per explicit user
request to include Aspire.

### 5. Command results are narrated back to the user via natural language, not swallowed silently

With Semantic Kernel's native function calling (Decision 6), each `GroceryListPlugin` function
returns a short descriptive string — e.g. `"Added 'Milk' to the list"`,
`"No item found matching 'eggs'"` — that the model incorporates into its next streamed reply.
This directly fixes the Python bug of catching parse/execution errors and only `print()`-ing
them: the user now finds out when a command didn't do what they expected, as part of the normal
chat response, with no new wire protocol or SSE event type needed — it rides on the existing
token stream. Unexpected exceptions (e.g. a DB failure, not a "not found" case) are still logged
via `ILogger` (Serilog) rather than surfaced to the model/user.

*Alternative considered and rejected:* keep the Python "swallow silently, only log" behavior for
consistency with today's app — rejected because tool-calling makes "tell the model what
happened so it can explain to the user" the natural, low-effort path; deliberately suppressing
that would mean writing extra code to strip results back out for no benefit.

### 6. Use Semantic Kernel with native tool-calling instead of a hand-rolled LLM client + trailing-JSON parser

`Microsoft.SemanticKernel` replaces the planned custom `ILlmClient`/factory pair (an earlier
draft of this plan mirrored the Python ABC+factory 1:1). SK's `Kernel` + chat-completion
connectors (`AddOpenAIChatCompletion`, `AddOllamaChatCompletion`) already provide the
provider-selection abstraction the custom `ILlmClient` would have hand-rolled, and SK's
`[KernelFunction]` plugin mechanism lets the model call `AddItem`/`RemoveItem`/`CheckItem`/
`UncheckItem` as typed tools during generation, instead of being asked to emit a specific JSON
array shape as plain text that the backend then has to strip markdown fences from and
`JSON.parse()`. This is materially more reliable (structured tool-call payloads from the model
API vs. hoping the model followed a prose instruction exactly) and more testable (plugin
functions are plain C# methods, unit-testable without touching an LLM at all — see Phase 3).

*Alternative considered and rejected:* port the Python `LLMClient` ABC + `create_llm_client`
factory + trailing-JSON-array parsing verbatim, as originally scoped for this phase — rejected
once tool-calling was explicitly requested, since re-implementing prompt-engineered JSON
parsing by hand would be strictly worse than using the tool-calling support the model APIs
already expose natively through Semantic Kernel.

### 7. Move `prototype/` to `docs/prototype/`, not `src/`

`prototype/` holds throwaway HTML/CSS mockups — not final code, and never destined for
production. Since the `src/` root exists specifically to hold buildable source (the .NET
backend and the React frontend), `prototype/` belongs alongside other non-source project
material under `docs/` instead.

*Alternative considered and rejected:* move it into `src/` for consistency with "everything
that isn't docs/config goes under src/" — rejected per the user's own framing: the dividing
line is "is this final code that ships," not "is this source-like content," and mockups fail
that test.

**Decided with the user.**

## Phases

Each phase after Phase 1 leaves `src/Server/` in a buildable, testable state — the Python
`Server/` keeps running untouched at the repo root until Phase 7, so the app is never without a
working backend mid-migration.

All C# code written in Phases 1–6 follows the `Flowing-CSharp` skill's conventions (naming,
member ordering, expression-bodied members, file-scoped namespaces, etc. — see also
`.claude/rules/csharp-specialist.md` in the FlowingDefault reference repo for the baseline this
skill builds on). All EF Core work — the `GroceryListAiDbContext`, entity configurations,
migrations, and the cross-provider `ProviderTypes` pattern (Decision 2) — follows the
`Flowing-EFCore` skill, which is also the source of the "SQLite for integration tests, Postgres
for production" convention behind Decision 3.

### Phase 1 — Solution scaffold

- Create `src/Server/GroceryListAi.sln` with `AppHost`, `ServiceDefaults`, `Api`, `Core`,
  `Tests` projects (empty/template state).
- Wire `DotNetEnv.Env.TraversePath().Load()` and Serilog (structured logging) in
  `GroceryListAi.Api/Program.cs`.
- `GroceryListAi.Core/GroceryListAiDbContext.cs` + `Models/Item.cs` (record) +
  `Configurations/ItemConfiguration.cs` (Fluent API, mirrors the current SQLAlchemy schema:
  `description varchar(255) not null`, `checked bool not null default false`, `created_at`,
  `updated_at`).
- Port the cross-provider migration pattern (Decision 2) and generate the initial migration.
- `DatabaseProvider` enum + `DATABASE_PROVIDER`/`CONNECTION_STRING` wiring in `Program.cs`
  (Decision 1).
- `GET /health` endpoint.

### Phase 2 — Items CRUD

- `ItemsController` with `GET /items`, `POST /items`, `DELETE /items/{id}`,
  `PATCH /items/{id}/checked` (fixed to return `404` on not-found).
- DTOs: `ItemCreateRequest`, `ItemCheckedUpdateRequest`, `ItemResponse` in
  `GroceryListAi.Core` or `GroceryListAi.Api` (co-locate with the template's convention of
  API-facing DTOs living in `Api`).
- `GroceryListAi.Tests`: xUnit + SQLite (file or `:memory:` with a kept-open connection) +
  `WebApplicationFactory<Program>`, covering CRUD parity with `test_items_api.py` (17 tests).

### Phase 3 — LLM abstraction via Semantic Kernel

- Add `Microsoft.SemanticKernel` + `Microsoft.SemanticKernel.Connectors.OpenAI` (stable) and
  `Microsoft.SemanticKernel.Connectors.Ollama` (experimental, gated behind the `SKEXP0070`
  diagnostic) to `GroceryListAi.Core`.
- `KernelFactory.Create(...)` builds a `Kernel` with the chat-completion connector selected by
  the `LLM` env var (`chatgpt` → `AddOpenAIChatCompletion`, `ollama` → `AddOllamaChatCompletion`)
  — structurally the same "factory selects an implementation from config" shape as the Python
  `create_llm_client`, but the implementations are now SK connectors instead of hand-rolled
  HTTP clients (Decision 6).
- `GroceryListPlugin` (`GroceryListAi.Core/Llm/GroceryListPlugin.cs`): a scoped class
  constructed with `GroceryListAiDbContext`, exposing four `[KernelFunction]`-attributed
  methods — `AddItem`, `RemoveItem`, `CheckItem`, `UncheckItem` — each with a `[Description]`
  telling the model when to call it. This is the direct replacement for the Python
  `SYSTEM_PROMPT`'s "Possible commands" JSON contract, now expressed as typed function
  signatures instead of a prose instruction. Each function returns a short descriptive result
  string rather than executing silently (Decision 5); the case-insensitive substring lookup
  ported from Python's `ilike` matching lives inside `RemoveItem`/`CheckItem`/`UncheckItem`.
- Trim `SYSTEM_PROMPT` down to scope/tone guidance only (e.g. "you manage a grocery list;
  refuse anything outside that scope") — the JSON answer-format instructions are dropped
  entirely, since tool-calling replaces that mechanism.
- Unit tests: `GroceryListPlugin` methods are tested directly against a SQLite-backed
  `GroceryListAiDbContext`, with **no LLM involved at all** — a genuine testability
  improvement over the Python original, where command execution was only reachable through the
  full `/chat` endpoint. `KernelFactory` is tested for correct connector selection per `LLM`
  value and for the missing-API-key failure case. Together these cover what `test_llm.py`'s
  25 tests covered structurally, even though the concrete assertions differ (there's no
  `stream_chat`/chunk-extraction logic left to test directly — that's now inside the SK
  connectors themselves).

### Phase 4 — Chat streaming endpoint

- `ChatController` `POST /chat`: accepts `{"message": "..."}` only. Checked `Web/api.ts`'s
  `chat()` function against both shapes the Python endpoint supported — it exclusively sends
  `{ message }` (a single string, no history) and reads the response as raw decoded bytes with
  no framing assumptions. The `{"messages": [...]}` array shape is not sent by anything in
  `Web/`, so it's the one actually dropped here — the inverse of this bullet's original
  assumption, corrected once the frontend was actually checked (see the updated Risk entry
  below).
- Builds a per-request `Kernel` via `KernelFactory`, imports the scoped `GroceryListPlugin`
  into it, then calls
  `IChatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory,
  new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
  kernel)`, streaming text chunks to the client as `text/event-stream` as they arrive.
- Semantic Kernel auto-invokes `GroceryListPlugin` functions as tool calls are detected
  mid-generation — this replaces the Python "accumulate full text → strip markdown fences →
  `JSON.parse` → dispatch" pipeline entirely; there is no separate post-stream parsing step.
- Log unexpected exceptions (e.g. a DB failure inside a plugin function) via `ILogger`
  (Serilog); expected "not found" cases are returned as plugin result strings, not thrown
  (Decision 5).
- Integration tests parity with `test_chat_api.py` (18 tests), using a fake/stub
  `IChatCompletionService` registered in a test `Kernel` that returns scripted streaming chunks
  — including tool-call content — so plugin invocation and streamed-text assembly can be
  tested deterministically without calling a real model.

### Phase 5 — Aspire orchestration

- `GroceryListAi.AppHost`: reference `Api`, add a Postgres container resource
  (`Aspire.Hosting.PostgreSQL`), wire `CONNECTION_STRING` from the Aspire resource into `Api`.
- Optionally add the `Web` frontend as an `Aspire.Hosting.NodeJs` resource for one-command local
  startup (`dotnet run --project GroceryListAi.AppHost`).
- `GroceryListAi.ServiceDefaults` added to `Api`: OpenTelemetry, health checks, resilience
  defaults (standard Aspire template project — `dotnet new aspire-servicedefaults`).

### Phase 6 — Docker / deployment

- New multi-stage `src/Server/Dockerfile` (SDK build stage → `aspnet` runtime stage), replacing
  `Server/Dockerfile`. Keeps `EXPOSE`/`HEALTHCHECK` semantics against `/health` and the existing
  `API_PORT` build-arg convention. Restores/builds only `GroceryListAi.Api.csproj` (which pulls
  in `Core`/`ServiceDefaults` transitively) rather than the whole solution, since `Tests` and
  `AppHost` aren't part of the runtime image's dependency graph.
- Updated root `docker-compose.yml`: backend build context → `src/Server`, dockerfile path
  updated, env vars switched to `DATABASE_PROVIDER`/`CONNECTION_STRING`/`CORS_ORIGINS` (dropping
  the now-irrelevant `PYTHONUNBUFFERED`/`SERVICE_NAME`/`SERVICE_DESCRIPTION`), and the
  `backend-logs` volume removed (Serilog logs to console only, matching the container-logs
  convention instead of a file volume). **Frontend build context stays at `./Web`** — it can't
  point at `src/Web` yet since that move doesn't happen until Phase 7; updating it now would
  break `docker-compose up`. Ports (5002 backend, 82 frontend) unchanged.
- Updated root `envexample.txt` (the actual docker-compose env template — `.env`/`.env.example`
  are off-limits to read/edit here, protected by a secrets hook) to match: `DATABASE_URL` →
  `DATABASE_PROVIDER` + `CONNECTION_STRING`, dropped `SERVICE_NAME`/`SERVICE_DESCRIPTION`, added
  `CORS_ORIGINS` (needed now since .NET actually enforces it, unlike Python).
- Verified `DotNetEnv.Env.TraversePath().Load()` is a safe no-op with no `.env` file anywhere in
  the ancestry (confirmed by publishing and running the binary from an isolated directory) — the
  container relies entirely on `docker-compose`'s `environment:` block, never a baked-in `.env`.

### Phase 7 — Repository restructuring cutover

- Moved `Web/` → `src/Web/` and `prototype/` → `docs/prototype/` (Decision 7) via a plain
  filesystem `mv` (not `git mv`) followed by `git add -A` from the repo root. This was
  deliberate, not a shortcut: `Web/` mixes tracked source with large gitignored directories
  (`node_modules/` at ~106 MB, `dist/`), and `git mv` only re-indexes tracked paths — it does not
  guarantee the untracked ones move with it. A plain `mv` renames the whole directory on the
  filesystem in one atomic operation (tracked and untracked content alike, since it's the same
  volume), and `git add -A` then stages the tracked files as deletions-at-old-path +
  additions-at-new-path. Verified with `git status` afterward that all 47 tracked files
  (43 under `Web/`, 4 under `prototype/`) were detected as clean renames (`R`), not separate
  add/delete pairs — content was untouched by the move, so git's similarity-based rename
  detection picked them up automatically, preserving `git log --follow` history.
- Updated the `AddNpmApp("web", "../../../Web", "dev")` relative path in
  `GroceryListAi.AppHost/AppHost.cs` to `../Web`.
- Updated `docker-compose.yml`'s frontend build context to `./src/Web`.
- Updated `build-web-linux.sh`/`build-web-windows.bat`: the `Web/package.json` existence check,
  `cd Web`, and the `cd ..` used to return to the repo root after the npm build all needed to
  change (`src/Web/package.json`, `cd src/Web`, `cd ../..` — one directory deeper than before).
- `README.md`/`CLAUDE.md` updates deferred to Phase 8, where `Flowing-update-docs` handles them
  holistically rather than splitting the rewrite across two phases.
- Moved `Server/` → `src/Legacy/` (same `mv` + `git add -A` approach; all 78 tracked files
  including the 47 above were confirmed as renames). Proceeded now rather than waiting for a
  Docker end-to-end smoke test specifically, since: the move is non-destructive and trivially
  reversible (git history, or just `mv` it back); "full test parity" is satisfied at the
  application level (36 xUnit tests covering health/items/chat, matching what Python's relevant
  suites covered); and Docker validation is a deployment-config concern, not an app-correctness
  one. If Docker turns up an issue, `src/Legacy/` still has the working Python reference.
- Verified after the moves: solution still builds clean, all 36 tests still pass, and
  `docker compose config` resolves both build contexts (`src/Server`, `src/Web`) correctly.

### Phase 8 — Docs cleanup

- Run the `Flowing-update-docs` skill to bring `README.md` and `CLAUDE.md` back in line with
  the real state of the code (tech stack table, Common Commands, Architecture Notes,
  repository structure — all now reflecting `.NET`/`src/`), instead of hand-editing them
  section by section.
- Retire or rewrite the already-stale `docs/API_CONTRACT.md` and (now archived, under
  `src/Legacy/`) `DATABASE_USAGE.md` — both currently describe an older in-memory-list API
  shape, per prior research. This is pre-existing drift, not something this migration
  introduces, but it should not be carried forward silently; `Flowing-update-docs` covers
  `README.md`/`CLAUDE.md` specifically, so these two need a manual pass.

## Affected Files

**Core (`src/Server/GroceryListAi.Core/`):** `Models/Item.cs`, `Configurations/ItemConfiguration.cs`,
`GroceryListAiDbContext.cs`, `Migrations/*`, `Migrations/ProviderTypes/*`, `Llm/*`,
`Services/ItemService.cs`.

**Api (`src/Server/GroceryListAi.Api/`):** `Program.cs`, `Controllers/ItemsController.cs`,
`Controllers/ChatController.cs`, DTOs.

**AppHost / ServiceDefaults (`src/Server/GroceryListAi.AppHost/`,
`.../GroceryListAi.ServiceDefaults/`):** new projects, template-generated + Postgres/Node
resource wiring.

**Tests (`src/Server/GroceryListAi.Tests/`):** new xUnit project mirroring the ported pytest
suites.

**Infra / deployment:** `src/Server/Dockerfile` (new), `docker-compose.yml` (root, paths
updated), `.env.example` (root, `DATABASE_URL` → `DATABASE_PROVIDER`/`CONNECTION_STRING`).

**Web:** `Web/` → `src/Web/` (move only, no code changes expected beyond verifying `api.ts`
still matches the `/chat` request/response shape).

**Archived:** `Server/` (Python) → `src/Legacy/` — moved, not deleted, in Phase 7 after parity is
confirmed.

**Docs:** `README.md`, `CLAUDE.md`, `docs/API_CONTRACT.md`, build scripts. `prototype/` →
`docs/prototype/` (move only, Decision 7).

## Risks

- **`ilike` substring matching isn't provider-portable** — Postgres's `ILIKE` has no direct EF
  Core equivalent that also works unmodified against Sqlite. Mitigated by testing the exact
  matching behavior against both providers in Phase 3's `GroceryListPlugin` unit tests and
  picking whichever EF Core case-insensitive pattern works identically on both (likely
  `.ToLower().Contains(...)` evaluated client-side is safest for parity, at the cost of not
  being a server-side query — acceptable given the table is small).
- **No existing migration history to diff against** — since Python never used Alembic, there is
  no prior schema version to validate the new EF Core migration against; the only source of
  truth is the current SQLAlchemy model, which has already been captured in this plan's
  "Current State" section.
- ~~Frontend request-shape drift~~ — resolved in Phase 4: `Web/api.ts`'s `chat()` only ever
  sends `{ message: "..." }` and parses the response as a raw undelimited byte stream (no SSE
  event framing assumed), so `ChatController` supports exactly that shape and the response
  `Content-Type` fix to `text/event-stream` is transparent to it (the client never reads that
  header).
- **Two-terminal → Aspire workflow change** — developers used to `uv run`/`npm run dev` need to
  switch habits to `dotnet run --project GroceryListAi.AppHost`; mitigated by updating
  `README.md`/`CLAUDE.md` Common Commands in Phase 8.
- **Semantic Kernel's Ollama connector is experimental** —
  `Microsoft.SemanticKernel.Connectors.Ollama` is gated behind the `SKEXP0070` diagnostic, and
  its tool-calling support depends on the local model actually being trained for function
  calling (e.g. `llama3.1`, `qwen2.5`) — smaller/older Ollama models may not reliably emit tool
  calls at all, unlike the Python version's plain-JSON-in-text approach which worked on any
  model regardless of function-calling training. Mitigated by documenting which local models
  are known to support tool calling well, and by explicitly testing the Ollama path in Phase 3
  rather than assuming OpenAI-path coverage transfers.

## Out of Scope

- Adding authentication/authorization (the Python app has none; the .NET port keeps the API
  open, matching current behavior).
- Redesigning the `/chat` wire protocol to surface mid-stream errors to the client (Decision 5)
  — a follow-up frontend+backend change if ever pursued.
- MySQL support — it is only doc-mentioned in the Python project and was never actually wired up
  (missing driver dependency); not carried into the .NET port.
- Adopting Aspire's `azd`/manifest-based cloud deployment — Aspire here is local-orchestration
  only, per Decision 4.
- Data migration from an existing SQLite/Postgres database — the schema is recreated fresh via
  EF Core migrations, matching how the Python app has no data-migration story today either.
- Rewriting the frontend (`Web/`) beyond its `src/Web/` relocation.
