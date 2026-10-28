import os
import fastapi as fastapi
from fastapi import Request, Depends
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import StreamingResponse
from sqlalchemy.orm import Session
import uvicorn
from dotenv import load_dotenv
import llm
import json

from Models import Item, get_db, init_db
from Models.schemas import ItemCreate, ItemResponse, ItemCheckedUpdate

load_dotenv()

# Initialize database tables
init_db()

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


@app.get("/items", response_model=list[ItemResponse])
def get_items(db: Session = Depends(get_db)):
    """Get all grocery list items"""
    print('Request get items')
    items = db.query(Item).all()
    print(f"Found {len(items)} items")
    return items


@app.post("/items", response_model=ItemResponse)
async def create_item(item_data: ItemCreate, db: Session = Depends(get_db)):
    """Create a new grocery list item"""
    print('Request create item')
    print(f"Item data: {item_data}")
    
    new_item = Item(
        description=item_data.description,
        checked=item_data.checked
    )
    db.add(new_item)
    db.commit()
    db.refresh(new_item)
    
    print(f"Created item: {new_item}")
    return new_item


@app.delete("/items/{item_id}")
def delete_item(item_id: int, db: Session = Depends(get_db)):
    """Delete a grocery list item"""
    print(f'Request delete item {item_id}')
    
    item = db.query(Item).filter(Item.id == item_id).first()
    
    if item:
        db.delete(item)
        db.commit()
        return {"message": "Item deleted"}
    else:
        return {"message": "Item not found"}


@app.patch("/items/{item_id}/checked", response_model=ItemResponse)
async def mark_item_as_checked(
    item_id: int, 
    update_data: ItemCheckedUpdate, 
    db: Session = Depends(get_db)
):
    """Update the checked status of a grocery list item"""
    print(f'Request mark item {item_id} as checked')
    print(f"Checked value: {update_data.checked}")
    
    item = db.query(Item).filter(Item.id == item_id).first()
    
    if item:
        item.checked = update_data.checked
        db.commit()
        db.refresh(item)
        return item
    else:
        return {"message": "Item not found"}
    
@app.post("/chat")
async def chat(request: Request, db: Session = Depends(get_db)):
    """Chat with AI to manage grocery list"""
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
        commands = ""
        async for chunk in llm.get_response(messages):
            commands = commands + chunk
            yield chunk
            
        # Parse and execute commands
        commands = commands.replace("```json", "").replace("```", "").strip()
        print(f"Command: {commands}")
        
        try:
            command_json = json.loads(commands)
            print(f"Command JSON: {command_json}")
            
            for command in command_json:
                command_type = command.get("command")
                value = command.get("value", "")
                
                if command_type == "AddItem":
                    print(f"Adding item: {value}")
                    new_item = Item(description=value, checked=False)
                    db.add(new_item)
                    db.commit()
                    
                elif command_type == "RemoveItem":
                    print(f"Removing item: {value}")
                    item = db.query(Item).filter(
                        Item.description.ilike(f"%{value}%")
                    ).first()
                    if item:
                        db.delete(item)
                        db.commit()
                    else:
                        print(f"Item not found: {value}")
                        
                elif command_type == "CheckItem":
                    print(f"Checking item: {value}")
                    item = db.query(Item).filter(
                        Item.description.ilike(f"%{value}%")
                    ).first()
                    if item:
                        item.checked = True
                        db.commit()
                    else:
                        print(f"Item not found: {value}")
                        
                elif command_type == "UncheckItem":
                    print(f"Unchecking item: {value}")
                    item = db.query(Item).filter(
                        Item.description.ilike(f"%{value}%")
                    ).first()
                    if item:
                        item.checked = False
                        db.commit()
                    else:
                        print(f"Item not found: {value}")
                        
        except json.JSONDecodeError as e:
            print(f"Error parsing JSON: {e}")
        except Exception as e:
            print(f"Error executing commands: {e}")

    return StreamingResponse(
        generate(),
        media_type="text/plain",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no"  # Disable buffering for nginx
        }
    )

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
