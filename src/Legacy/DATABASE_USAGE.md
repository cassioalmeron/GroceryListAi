# Database Usage Guide

## Setup

### 1. Install SQLAlchemy

```bash
pip install sqlalchemy
```

Or add to your `pyproject.toml`:
```toml
[project]
dependencies = [
    "sqlalchemy>=2.0.0",
]
```

### 2. Configure Database URL

Add to your `.env` file:

```env
# SQLite (default)
DATABASE_URL=sqlite:///./grocery_list.db

# PostgreSQL
# DATABASE_URL=postgresql://user:password@localhost/grocery_list

# MySQL
# DATABASE_URL=mysql+pymysql://user:password@localhost/grocery_list
```

### 3. Initialize Database

```python
from Models import init_db

# Create all tables
init_db()
```

## Using with FastAPI

### Example: Migrate `api.py` to use database

```python
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

# Initialize database
init_db()

app = fastapi.FastAPI()

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/health")
def health():
    return {"message": "Grocery List API is running"}


# Get all items
@app.get("/items", response_model=list[ItemResponse])
def get_items(db: Session = Depends(get_db)):
    items = db.query(Item).all()
    return items


# Create item
@app.post("/items", response_model=ItemResponse)
async def create_item(item_data: ItemCreate, db: Session = Depends(get_db)):
    print('Request create item')
    
    new_item = Item(
        description=item_data.description,
        checked=item_data.checked
    )
    db.add(new_item)
    db.commit()
    db.refresh(new_item)
    
    return new_item


# Delete item
@app.delete("/items/{item_id}")
def delete_item(item_id: int, db: Session = Depends(get_db)):
    print(f'Request delete item {item_id}')
    
    item = db.query(Item).filter(Item.id == item_id).first()
    
    if item:
        db.delete(item)
        db.commit()
        return {"message": "Item deleted"}
    else:
        return {"message": "Item not found"}


# Update item checked status
@app.patch("/items/{item_id}/checked", response_model=ItemResponse)
async def mark_item_as_checked(
    item_id: int, 
    update_data: ItemCheckedUpdate, 
    db: Session = Depends(get_db)
):
    print(f'Request mark item {item_id} as checked')
    
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
    print(f'Request chat')
    
    body = await request.json()
    
    if "messages" in body:
        messages = body.get("messages", [])
        print(f"Messages history: {len(messages)} messages")
    else:
        message = body.get("message", "")
        messages = [{"role": "user", "content": message}]
        print(f"Single message: {message}")
    
    print(f"Full conversation: {messages}")

    async def generate():
        commands = ""
        async for chunk in llm.get_response(messages):
            commands = commands + chunk
            yield chunk
            
        commands = commands.replace("```json", "").replace("```", "")
        print(f"Command: {commands}")
        command_json = json.loads(commands)
        print(f"Command JSON: {command_json}")
        
        for command in command_json:
            if command["command"] == "AddItem":
                new_item = Item(description=command["value"], checked=False)
                db.add(new_item)
                db.commit()
                
            elif command["command"] == "RemoveItem":
                item = db.query(Item).filter(
                    Item.description.ilike(f"%{command['value']}%")
                ).first()
                if item:
                    db.delete(item)
                    db.commit()
                    
            elif command["command"] == "CheckItem":
                item = db.query(Item).filter(
                    Item.description.ilike(f"%{command['value']}%")
                ).first()
                if item:
                    item.checked = True
                    db.commit()
                    
            elif command["command"] == "UncheckItem":
                item = db.query(Item).filter(
                    Item.description.ilike(f"%{command['value']}%")
                ).first()
                if item:
                    item.checked = False
                    db.commit()

    return StreamingResponse(
        generate(), 
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no"
        }
    )


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
```

## Direct Database Operations (CRUD)

### Create

```python
from Models import SessionLocal, Item

db = SessionLocal()

# Create new item
new_item = Item(description="Milk", checked=False)
db.add(new_item)
db.commit()
db.refresh(new_item)

print(new_item.id)  # Auto-generated ID
```

### Read

```python
# Get all items
items = db.query(Item).all()

# Get item by ID
item = db.query(Item).filter(Item.id == 1).first()

# Get unchecked items
unchecked = db.query(Item).filter(Item.checked == False).all()

# Search by description
milk_items = db.query(Item).filter(Item.description.like("%Milk%")).all()
```

### Update

```python
# Update item
item = db.query(Item).filter(Item.id == 1).first()
if item:
    item.checked = True
    db.commit()
    db.refresh(item)
```

### Delete

```python
# Delete item
item = db.query(Item).filter(Item.id == 1).first()
if item:
    db.delete(item)
    db.commit()
```

## Model Structure

### Item Model (`Models/item.py`)

```python
class Item(Base):
    __tablename__ = "items"
    
    id: int              # Primary key, auto-increment
    description: str     # Item description (max 255 chars)
    checked: bool        # Check status (default: False)
    created_at: datetime # Creation timestamp (auto)
    updated_at: datetime # Update timestamp (auto)
```

### Methods

- `__repr__()`: String representation
- `to_dict()`: Convert to dictionary

## Pydantic Schemas

Located in `Models/schemas.py`:

- `ItemBase`: Base schema with common fields
- `ItemCreate`: For creating items (POST requests)
- `ItemUpdate`: For updating items (PUT/PATCH requests)
- `ItemResponse`: For API responses (includes ID and timestamps)
- `ItemCheckedUpdate`: For updating only checked status

## Benefits of Using Database

1. **Persistence**: Data survives server restarts
2. **Concurrent Access**: Multiple users can access safely
3. **Validation**: SQLAlchemy provides type checking
4. **Timestamps**: Automatic created_at and updated_at
5. **Relationships**: Easy to add related tables later
6. **Migration**: Can use Alembic for schema changes
7. **Query Power**: Complex filtering and sorting

## Next Steps

1. Install SQLAlchemy: `pip install sqlalchemy`
2. Update `.env` with DATABASE_URL
3. Replace in-memory list with database queries
4. Run `init_db()` to create tables
5. Test with your API

## Migration Tools (Optional)

For production, consider using Alembic for migrations:

```bash
pip install alembic
alembic init alembic
alembic revision --autogenerate -m "Create items table"
alembic upgrade head
```

