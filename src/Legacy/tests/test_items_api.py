"""
Tests for the items API endpoints
"""
import pytest
from Models import Item


class TestHealthEndpoint:
    """Test the health check endpoint"""

    def test_health_endpoint(self, client):
        """Test GET /health returns status"""
        response = client.get("/health")
        assert response.status_code == 200
        assert response.json() == {"message": "Grocery List API is running"}


class TestGetItems:
    """Test the GET /items endpoint"""

    def test_get_items_empty_list(self, client, db_session):
        """Test getting items when list is empty"""
        response = client.get("/items")
        assert response.status_code == 200
        assert response.json() == []

    def test_get_items_with_items(self, client, db_session):
        """Test getting items when items exist"""
        # Create test items
        item1 = Item(description="Apples", checked=False)
        item2 = Item(description="Oranges", checked=True)
        db_session.add(item1)
        db_session.add(item2)
        db_session.commit()

        response = client.get("/items")
        assert response.status_code == 200
        items = response.json()
        assert len(items) == 2
        assert items[0]["description"] == "Apples"
        assert items[0]["checked"] == False
        assert items[1]["description"] == "Oranges"
        assert items[1]["checked"] == True

    def test_get_items_contains_all_fields(self, client, db_session):
        """Test that returned items have all required fields"""
        item = Item(description="Test Item", checked=False)
        db_session.add(item)
        db_session.commit()

        response = client.get("/items")
        assert response.status_code == 200
        items = response.json()
        assert len(items) == 1

        item_data = items[0]
        assert "id" in item_data
        assert "description" in item_data
        assert "checked" in item_data
        assert "created_at" in item_data
        assert "updated_at" in item_data


class TestCreateItem:
    """Test the POST /items endpoint"""

    def test_create_item_success(self, client, db_session):
        """Test creating a new item successfully"""
        payload = {"description": "Milk", "checked": False}
        response = client.post("/items", json=payload)

        assert response.status_code == 200
        data = response.json()
        assert data["description"] == "Milk"
        assert data["checked"] == False
        assert "id" in data
        assert "created_at" in data
        assert "updated_at" in data

        # Verify item was saved to database
        items = db_session.query(Item).all()
        assert len(items) == 1
        assert items[0].description == "Milk"

    def test_create_item_with_checked_true(self, client, db_session):
        """Test creating an item with checked status as true"""
        payload = {"description": "Bread", "checked": True}
        response = client.post("/items", json=payload)

        assert response.status_code == 200
        data = response.json()
        assert data["description"] == "Bread"
        assert data["checked"] == True

    def test_create_item_empty_description(self, client):
        """Test creating an item with empty description fails"""
        payload = {"description": "", "checked": False}
        response = client.post("/items", json=payload)

        assert response.status_code == 422  # Validation error

    def test_create_item_missing_description(self, client):
        """Test creating an item without description fails"""
        payload = {"checked": False}
        response = client.post("/items", json=payload)

        assert response.status_code == 422  # Validation error

    def test_create_item_description_too_long(self, client):
        """Test creating an item with description over 255 characters fails"""
        long_description = "a" * 256
        payload = {"description": long_description, "checked": False}
        response = client.post("/items", json=payload)

        assert response.status_code == 422  # Validation error

    def test_create_multiple_items(self, client, db_session):
        """Test creating multiple items"""
        items_to_create = [
            {"description": "Item 1", "checked": False},
            {"description": "Item 2", "checked": True},
            {"description": "Item 3", "checked": False},
        ]

        for item_data in items_to_create:
            response = client.post("/items", json=item_data)
            assert response.status_code == 200

        # Verify all items were saved
        items = db_session.query(Item).all()
        assert len(items) == 3


class TestDeleteItem:
    """Test the DELETE /items/{item_id} endpoint"""

    def test_delete_existing_item(self, client, db_session):
        """Test deleting an existing item"""
        # Create an item
        item = Item(description="To Delete", checked=False)
        db_session.add(item)
        db_session.commit()
        item_id = item.id

        # Delete the item
        response = client.delete(f"/items/{item_id}")
        assert response.status_code == 200
        assert response.json() == {"message": "Item deleted"}

        # Verify item was deleted
        remaining = db_session.query(Item).all()
        assert len(remaining) == 0

    def test_delete_non_existing_item(self, client):
        """Test deleting a non-existing item"""
        response = client.delete("/items/999")
        assert response.status_code == 200
        assert response.json() == {"message": "Item not found"}

    def test_delete_item_with_invalid_id(self, client):
        """Test deleting with invalid item ID format"""
        response = client.delete("/items/invalid")
        assert response.status_code == 422  # Validation error


class TestUpdateItemCheckedStatus:
    """Test the PATCH /items/{item_id}/checked endpoint"""

    def test_mark_item_as_checked(self, client, db_session):
        """Test marking an unchecked item as checked"""
        # Create an unchecked item
        item = Item(description="To Check", checked=False)
        db_session.add(item)
        db_session.commit()
        item_id = item.id

        # Mark as checked
        payload = {"checked": True}
        response = client.patch(f"/items/{item_id}/checked", json=payload)

        assert response.status_code == 200
        data = response.json()
        assert data["id"] == item_id
        assert data["checked"] == True

        # Verify in database
        updated_item = db_session.query(Item).filter(Item.id == item_id).first()
        assert updated_item.checked == True

    def test_mark_item_as_unchecked(self, client, db_session):
        """Test marking a checked item as unchecked"""
        # Create a checked item
        item = Item(description="To Uncheck", checked=True)
        db_session.add(item)
        db_session.commit()
        item_id = item.id

        # Mark as unchecked
        payload = {"checked": False}
        response = client.patch(f"/items/{item_id}/checked", json=payload)

        assert response.status_code == 200
        data = response.json()
        assert data["checked"] == False

    def test_update_item_with_invalid_id(self, client):
        """Test updating with invalid item ID format"""
        payload = {"checked": True}
        response = client.patch("/items/invalid/checked", json=payload)

        assert response.status_code == 422  # Validation error

    def test_update_without_checked_field(self, client, db_session):
        """Test updating without the checked field fails"""
        # Create an item
        item = Item(description="Test", checked=False)
        db_session.add(item)
        db_session.commit()

        # Try to update without checked field
        payload = {}
        response = client.patch(f"/items/{item.id}/checked", json=payload)

        assert response.status_code == 422  # Validation error
