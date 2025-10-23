import os
import fastapi as fastapi
from fastapi import Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import StreamingResponse
import uvicorn
from dotenv import load_dotenv
import llm
import json

load_dotenv()

app = fastapi.FastAPI()

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Allows all origins
    allow_credentials=True,
    allow_methods=["*"],  # Allows all methods
    allow_headers=["*"],  # Allows all headers
)

@app.get("/health")
def health():
    return {"message": "Grocery List API is running"}


items = []

@app.get("/items")
def get_items():
    print(items)
    print('Requestes get items')
    return items

@app.post("/items")
async def create_item(request: Request):
    print(request)
    print('Request create item')
    
    # Parse the JSON body
    body = await request.json()
    print(f"Body: {body}")
    
    next_id = len(items) + 1
    item = {
        "id": next_id,
        "description": body["description"],
        "checked": False,
    }
    items.append(item)
    return {"message": "Item created", "item": item}

@app.delete("/items/{item_id}")
def delete_item(item_id: int):
    print(f'Request delete item {item_id}')
    
    item_to_delete = next((item for item in items if item["id"] == item_id), None)
    
    if item_to_delete:
        items.remove(item_to_delete)
        return {"message": "Item deleted"}
    else:
        return {"message": "Item not found"}
    
@app.patch("/items/{item_id}/checked")
async def mark_item_as_checked(item_id: int, request: Request):
    print(f'Request mark item {item_id} as checked')
    
    # Parse the JSON body to get the checked value
    body = await request.json()
    checked = body.get("checked", False)
    print(f"Checked value: {checked}")
    
    # Find the item to update
    item_to_update = next((item for item in items if item["id"] == item_id), None)
    
    if item_to_update:
        item_to_update["checked"] = checked
        return {"message": "Item updated", "item": item_to_update}
    else:
        return {"message": "Item not found"}
    
@app.post("/chat")
async def chat(request: Request):
    
    print(f'Request chat')
    
    # Parse the JSON body to get the message and history
    body = await request.json()
    
    # Support both formats: single message or messages array
    if "messages" in body:
        messages = body.get("messages", [])
        print(f"Messages history: {len(messages)} messages")
    else:
        # Legacy format with single message
        message = body.get("message", "")
        messages = [{"role": "user", "content": message}]
        print(f"Single message: {message}")
    
    print(f"Full conversation: {messages}")

    async def generate():
        commands = "";
        async for chunk in llm.get_response(messages):
            commands = commands + chunk
            yield chunk
            
        commands = commands.replace("```json", "").replace("```", "")
        print(f"Command: {commands}")
        command_json = json.loads(commands)
        print(f"Command JSON: {command_json}")
        for command in command_json:
            if command["command"] == "AddItem":
                items.append({"id": len(items) + 1, "description": command["value"], "checked": False})
            elif command["command"] == "RemoveItem":
                items.remove(next((item for item in items if item["description"].lower() == command["value"].lower()), None))
            elif command["command"] == "CheckItem":
                print(f"Check item: {command['value']}")
                item = next((item for item in items if item["description"].lower() == command["value"].lower()), None)
                print(f"Item: {item}")
                item["checked"] = True
            elif command["command"] == "UncheckItem":
                print(f"Uncheck item: {command['value']}")
                next((item for item in items if item["description"].lower() == command["value"].lower()), None)["checked"] = False

    return StreamingResponse(
        generate(), 
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no"  # Disable buffering for nginx
        }
    )

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
