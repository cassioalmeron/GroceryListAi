# Grocery List API Contract

## Base URL
```
http://localhost:8000
```

## Headers
All requests should include:
```
Content-Type: application/json
```

---

## Endpoints

### 1. Health Check

**GET** `/health`

Check if the API is running.

#### Request
No parameters required.

#### Response
```json
{
  "message": "Grocery List API is running"
}
```

#### Status Codes
- `200 OK`: API is running

---

### 2. Get All Items

**GET** `/items`

Retrieve all grocery list items.

#### Request
No parameters required.

#### Response
```json
[
  {
    "id": 1,
    "description": "Milk",
    "checked": false
  },
  {
    "id": 2,
    "description": "Bread",
    "checked": true
  }
]
```

#### Status Codes
- `200 OK`: Items retrieved successfully

---

### 3. Create Item

**POST** `/items`

Create a new grocery list item.

#### Request Body
```json
{
  "description": "Eggs"
}
```

#### Response
```json
{
  "message": "Item created",
  "item": {
    "id": 3,
    "description": "Eggs",
    "checked": false
  }
}
```

#### Status Codes
- `200 OK`: Item created successfully

---

### 4. Delete Item

**DELETE** `/items/{item_id}`

Delete a specific grocery list item.

#### URL Parameters
- `item_id` (integer, required): The ID of the item to delete

#### Request
No body required.

#### Response (Success)
```json
{
  "message": "Item deleted"
}
```

#### Response (Not Found)
```json
{
  "message": "Item not found"
}
```

#### Status Codes
- `200 OK`: Item deleted or not found

#### Example
```
DELETE /items/1
```

---

### 5. Update Item Checked Status

**PATCH** `/items/{item_id}/checked`

Update the checked status of a specific item.

#### URL Parameters
- `item_id` (integer, required): The ID of the item to update

#### Request Body
```json
{
  "checked": true
}
```

#### Response (Success)
```json
{
  "message": "Item updated",
  "item": {
    "id": 1,
    "description": "Milk",
    "checked": true
  }
}
```

#### Response (Not Found)
```json
{
  "message": "Item not found"
}
```

#### Status Codes
- `200 OK`: Item updated or not found

#### Example
```
PATCH /items/1/checked
Body: {"checked": true}
```

---

### 6. Chat with AI (Streaming)

**POST** `/chat`

Send a message to the AI and receive a streaming response.

#### Request Body (Format 1: Single Message)
```json
{
  "message": "What should I buy for dinner?"
}
```

#### Request Body (Format 2: Full Conversation History)
```json
{
  "messages": [
    {
      "role": "user",
      "content": "What should I buy for dinner?"
    },
    {
      "role": "assistant",
      "content": "I recommend buying chicken, vegetables, and rice."
    },
    {
      "role": "user",
      "content": "What vegetables would be best?"
    }
  ]
}
```

#### Message Roles
- `user`: Message from the user
- `assistant`: Response from the AI
- `system`: (Optional) System instructions

#### Response
The response is a **Server-Sent Events (SSE)** stream with `Content-Type: text/event-stream`.

The AI response is streamed in chunks as plain text.

#### Response Headers
```
Content-Type: text/event-stream
Cache-Control: no-cache
Connection: keep-alive
X-Accel-Buffering: no
```

#### Example Response Stream
```
Hello
 there
!
 I
 recommend
 buying
 carrots
,
 broccoli
,
 and
 bell
 peppers
.
```

#### Status Codes
- `200 OK`: Stream started successfully

#### Error Handling
If an error occurs during streaming, an error message will be sent in the stream:
```
Error: <error message>
```

#### JavaScript Example
```javascript
const response = await fetch('/chat', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    messages: [
      { role: 'user', content: 'What should I buy?' }
    ]
  })
});

const reader = response.body.getReader();
const decoder = new TextDecoder();

while (true) {
  const { done, value } = await reader.read();
  if (done) break;
  
  const chunk = decoder.decode(value);
  console.log('Received:', chunk);
}
```

---

## Environment Variables

The API requires the following environment variables:

### For ChatGPT
```env
LLM=chatgpt
MODEL=gpt-3.5-turbo
OPENAI_API_KEY=your_api_key_here
```

### For Ollama
```env
LLM=ollama
MODEL=llama2
# No API key required, Ollama runs locally
```

---

## CORS

CORS is enabled for all origins (`*`). The following headers are allowed:
- All origins
- All methods (GET, POST, PATCH, DELETE, etc.)
- All headers
- Credentials are allowed

---

## Data Models

### Item
```typescript
{
  id: number;           // Auto-incremented ID
  description: string;  // Item description
  checked: boolean;     // Whether item is checked
}
```

### Message
```typescript
{
  role: "user" | "assistant" | "system";
  content: string;
}
```

---

## Error Handling

### Chat Endpoint Errors
Errors in the chat endpoint are returned as part of the stream:
```
Error: <error description>
```

### General Errors
The API may return standard HTTP error codes for invalid requests.

---

## Notes

1. **In-Memory Storage**: Items are stored in memory and will be lost when the server restarts.
2. **Streaming**: The `/chat` endpoint uses Server-Sent Events for streaming responses.
3. **AI Provider**: The API supports both ChatGPT (OpenAI) and Ollama as AI providers.
4. **Conversation History**: To maintain conversation context, send the full message history in the `/chat` request.

