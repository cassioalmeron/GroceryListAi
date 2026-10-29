# Plan: Migrate Backend from SQLite to PostgreSQL

**Status:** Implemented (28/10/2026) — production runs on Postgres

## Context

The GroceryListAi backend currently uses SQLite for local development convenience. Moving to PostgreSQL prepares the project for production use with better concurrency, data integrity, and cloud hosting compatibility. No data migration is needed — the database will start fresh.

---

## Critical Files to Modify

| File | Change |
|---|---|
| `Server/pyproject.toml` | Add `psycopg2-binary` dependency |
| `Server/Models/database.py` | Remove SQLite-specific `connect_args` |
| `Server/.env.example` | Update example `DATABASE_URL` to PostgreSQL |
| `Server/Dockerfile` | Remove SQLite references, verify no `.db` file usage |
| `Server/conftest.py` | No change needed — tests keep using SQLite in-memory |

---

## Implementation Steps

### 1. Add PostgreSQL Driver Dependency

In `Server/pyproject.toml`, add `psycopg2-binary` to the dependencies list:

```toml
dependencies = [
    ...
    "psycopg2-binary>=2.9",
]
```

Then run `uv lock` to regenerate the lock file.

### 2. Update `Models/database.py`

In `Server/Models/database.py`, the `connect_args={"check_same_thread": False}` is SQLite-specific. Since SQLite is no longer used, simplify `create_engine`:

```python
engine = create_engine(DATABASE_URL)
```

The `DATABASE_URL` default fallback to SQLite can also be removed:

```python
DATABASE_URL = os.getenv("DATABASE_URL")
if not DATABASE_URL:
    raise RuntimeError("DATABASE_URL environment variable is not set")
```

### 3. Update `.env.example`

Update `Server/.env.example` to show PostgreSQL as the primary example:

```env
DATABASE_URL=postgresql://user:password@localhost:5432/grocery_list
```

### 4. Update Developer's `.env`

The local `Server/.env` must be updated (manually by each developer) to use a real PostgreSQL connection string:

```env
DATABASE_URL=postgresql://postgres:password@localhost:5432/grocery_list
```

This file is gitignored and must be configured per environment.

### 5. Update `Dockerfile`

`Server/Dockerfile` installs dependencies via `pip install .` from `pyproject.toml`. With `psycopg2-binary` added, no extra system libraries are needed (binaries are self-contained). No structural changes required to the Dockerfile.

### 6. Update `docker-compose.yml`

The root `docker-compose.yml` already forwards `DATABASE_URL` from `.env` to the backend container — no new service is added. PostgreSQL is **external** (managed cloud DB, local install, etc.) and supplied via the `DATABASE_URL` environment variable.

Review whether the `backend-data:/data` volume is still needed. If it was only used to persist the SQLite `.db` file, it can be removed:

```yaml
# Remove this volume mount if no longer needed:
# - backend-data:/data

# And remove from the volumes section:
# backend-data:
#   driver: local
```

Update the root `.env.example` to include a PostgreSQL `DATABASE_URL`:

```env
DATABASE_URL=postgresql://user:password@hostname:5432/grocery_list
```

### 6. Keep Test Suite on SQLite (No Change to `conftest.py`)

The tests in `Server/conftest.py` use `sqlite:///:memory:`, which is fast, isolated, and sufficient for unit testing. Since SQLAlchemy is DB-agnostic, no test logic changes are needed.

> Note: If integration/E2E tests are added in the future, they should target a real PostgreSQL instance (e.g., via `pytest-postgresql` or a Docker-based test DB).

---

## Optional: Add Alembic for Schema Migrations

The current `init_db()` uses `Base.metadata.create_all()`. For production-grade schema management:

1. Add `alembic>=1.13` to dependencies
2. Run `alembic init alembic` in the `Server/` directory
3. Configure `alembic/env.py` to import `Base` from `Models`
4. Generate the initial revision: `alembic revision --autogenerate -m "Initial schema"`
5. Apply: `alembic upgrade head`

This is **optional** — decide after the core migration is confirmed working.

---

## Verification

1. Install dependencies: `uv sync`
2. Start a local PostgreSQL instance (Docker example):
   ```bash
   docker run --name grocery-pg \
     -e POSTGRES_PASSWORD=password \
     -e POSTGRES_DB=grocery_list \
     -p 5432:5432 \
     -d postgres:16
   ```
3. Set `DATABASE_URL` in `Server/.env`
4. Run the server: `uv run uvicorn api:app --reload`
5. On startup, `init_db()` will create the `items` table in PostgreSQL automatically
6. Verify via `psql` or a DB client that the `items` table was created
7. Run the test suite: `uv run pytest` — all tests should still pass (they use SQLite in-memory)
8. Call `POST /items` and `GET /items` via the API to confirm end-to-end functionality

---

## Summary of Changes

| File | Action |
|---|---|
| `Server/pyproject.toml` | Add `psycopg2-binary` |
| `Server/uv.lock` | Regenerated via `uv sync` |
| `Server/Models/database.py` | Remove SQLite-specific `connect_args`; raise error if `DATABASE_URL` is unset |
| `Server/.env.example` | Switch to PostgreSQL URL example |
| `Server/Dockerfile` | No changes needed |
| `docker-compose.yml` | Remove `backend-data` volume if SQLite-only; update `.env.example` with PostgreSQL URL |
