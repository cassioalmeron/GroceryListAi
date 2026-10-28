# Grocery List API Contract

## Base URL

The API listens on `http://localhost:5071` when started with `dotnet run`, and on `API_PORT` in Docker. In Docker Compose the frontend's nginx also proxies `/api/` to the backend.

All request bodies are JSON (`Content-Type: application/json`). JSON property names are camelCase.

## Item

```json
{
  "id": 1,
  "description": "Milk",
  "checked": false,
  "createdAt": "2026-10-28T15:03:47.123",
  "updatedAt": "2026-10-28T15:03:47.123"
}
```

Timestamps are UTC. `updatedAt` changes whenever the item is modified.

## Errors

| Status | Body |
|---|---|
| `400 Bad Request` | Validation failure — ASP.NET Core `ValidationProblemDetails` (`errors` keyed by field) |
| `404 Not Found` | `{ "message": "Item not found" }` |
| `500 Internal Server Error` | `{ "message": "Error ..." }` — details are logged on the server, not returned |

---

## GET /health

Health check.

**Response `200`**
```json
{ "message": "Grocery List API is running" }
```

## GET /api/items

Lists all items in insertion order.

**Response `200`** — array of [Item](#item).

## POST /api/items

Creates an item. `checked` starts as `false`.

**Request**
```json
{ "description": "Milk" }
```

`description` is required, 1–255 characters.

**Response `200`** — the created [Item](#item).
**Response `400`** — missing, empty or too-long `description`.

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

Sends a message to the assistant. The LLM can call tools that add, remove, check or uncheck items, then replies in natural language.

**Request**
```json
{ "message": "Add milk and eggs" }
```

**Response `200`** — `Content-Type: text/event-stream`. The body is the assistant's reply as **raw text chunks** written as they arrive (no `data:` framing); read it with a stream reader and concatenate the chunks.

If the LLM fails before anything is written, the status is `500`. If it fails mid-stream, the response simply ends early; the error is only logged on the server.

After a chat call, re-fetch `GET /api/items` to see the changes the assistant made.
