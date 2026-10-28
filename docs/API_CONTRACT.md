# Grocery List API Contract

## Base URL

The API listens on `http://localhost:5071` when started with `dotnet run`, and on `API_PORT` in Docker. In Docker Compose the frontend's nginx also proxies `/api/` to the backend.

All request bodies are JSON (`Content-Type: application/json`). JSON property names are camelCase.

## List

```json
{
  "id": 1,
  "name": "General",
  "createdAt": "2026-10-28T15:03:47.123",
  "updatedAt": "2026-10-28T15:03:47.123"
}
```

There is always at least one list. A list named `General` is created by the database migration.

## Item

```json
{
  "id": 1,
  "description": "Milk",
  "checked": false,
  "listId": 1,
  "createdAt": "2026-10-28T15:03:47.123",
  "updatedAt": "2026-10-28T15:03:47.123"
}
```

Every item belongs to exactly one list. Timestamps are UTC. `updatedAt` changes whenever the item is modified.

## Errors

| Status | Body |
|---|---|
| `400 Bad Request` | Validation failure — ASP.NET Core `ValidationProblemDetails` (`errors` keyed by field) |
| `404 Not Found` | `{ "message": "Item not found" }` or `{ "message": "List not found" }` |
| `400 Bad Request` | `{ "message": "..." }` — business rule refused: duplicate list name, or deleting the last list |
| `500 Internal Server Error` | `{ "message": "Error ..." }` — details are logged on the server, not returned |

---

## GET /health

Health check.

**Response `200`**
```json
{ "message": "Grocery List API is running" }
```

## GET /api/lists

Lists all lists, ordered by `id`.

**Response `200`** — array of [List](#list).

## POST /api/lists

Creates a list.

**Request**
```json
{ "name": "Pharmacy" }
```

`name` is required, 1–100 characters, and is trimmed. Names are unique, ignoring case.

**Response `200`** — the created [List](#list).
**Response `400`** — missing, empty or too-long `name`.
**Response `400`** — `{ "message": "A list named 'Pharmacy' already exists" }`.

## PUT /api/lists/{id}

Renames a list. Same `name` rules as `POST /api/lists`; a list may be renamed to its own name with different casing.

**Request**
```json
{ "name": "Drugstore" }
```

**Response `200`** — the renamed [List](#list).
**Response `400`** — invalid `name`.
**Response `404`** — no list with that `id`.
**Response `400`** — another list already has that name (`message` says so).

## DELETE /api/lists/{id}

Deletes a list **and all its items**.

**Response `200`**
```json
{ "message": "List deleted" }
```

**Response `404`** — no list with that `id`.
**Response `400`** — `{ "message": "The last list cannot be deleted" }`.

## GET /api/items?listId={listId}

Lists the items of one list in insertion order. `listId` is required.

**Response `200`** — array of [Item](#item).
**Response `400`** — `listId` missing.
**Response `404`** — no list with that `listId`.

## POST /api/items

Creates an item in a list. `checked` starts as `false`.

**Request**
```json
{ "description": "Milk", "listId": 1 }
```

`description` is required, 1–255 characters. `listId` is required.

**Response `200`** — the created [Item](#item).
**Response `400`** — missing, empty or too-long `description`, or missing `listId`.
**Response `404`** — no list with that `listId`.

## PATCH /api/items/{id}/checked

Sets an item's checked state.

**Request**
```json
{ "checked": true }
```

**Response `200`** — the updated [Item](#item).
**Response `404`** — no item with that `id`.

## DELETE /api/items/{id}

Deletes an item.

**Response `200`**
```json
{ "message": "Item deleted" }
```

**Response `404`** — no item with that `id` (also for a non-integer `id`).

## POST /api/chat

Sends a message to the assistant. The LLM can call tools that add, remove, check or uncheck items **of the given list only**, then replies in natural language. The list is fixed by the request; the assistant cannot act on another list.

**Request**
```json
{ "message": "Add milk and eggs", "listId": 1 }
```

**Response `400`** — missing `message` or `listId`.
**Response `404`** — no list with that `listId` (returned before any streaming starts).

**Response `200`** — `Content-Type: text/event-stream`. The body is the assistant's reply as **raw text chunks** written as they arrive (no `data:` framing); read it with a stream reader and concatenate the chunks.

If the LLM fails before anything is written, the status is `500`. If it fails mid-stream, the response simply ends early; the error is only logged on the server.

After a chat call, re-fetch `GET /api/items?listId=` to see the changes the assistant made.
