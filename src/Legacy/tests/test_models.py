"""
Tests for the Item model
"""
import pytest
from datetime import datetime
from Models import Item


class TestItemModel:
    """Test the Item ORM model"""

    def test_item_creation(self, db_session):
        """Test creating an Item instance"""
        item = Item(description="Test Item", checked=False)
        db_session.add(item)
        db_session.commit()

        assert item.id is not None
        assert item.description == "Test Item"
        assert item.checked == False
        assert item.created_at is not None
        assert item.updated_at is not None

    def test_item_default_checked_value(self, db_session):
        """Test that checked defaults to False"""
        item = Item(description="Test")
        db_session.add(item)
        db_session.commit()

        assert item.checked == False

    def test_item_with_checked_true(self, db_session):
        """Test creating item with checked=True"""
        item = Item(description="Test", checked=True)
        db_session.add(item)
        db_session.commit()

        assert item.checked == True

    def test_item_timestamps(self, db_session):
        """Test that timestamps are set correctly"""
        item = Item(description="Test", checked=False)
        db_session.add(item)
        db_session.commit()

        assert item.created_at is not None
        assert item.updated_at is not None
        # Check that both timestamps exist and are datetime objects
        assert isinstance(item.created_at, datetime) or hasattr(item.created_at, 'isoformat')
        assert isinstance(item.updated_at, datetime) or hasattr(item.updated_at, 'isoformat')

    def test_item_repr(self, db_session):
        """Test the string representation of Item"""
        item = Item(description="Test Item", checked=True)
        db_session.add(item)
        db_session.commit()

        repr_str = repr(item)
        assert "Item" in repr_str
        assert "Test Item" in repr_str
        assert "True" in repr_str

    def test_item_to_dict(self, db_session):
        """Test converting item to dictionary"""
        item = Item(description="Test Item", checked=True)
        db_session.add(item)
        db_session.commit()

        item_dict = item.to_dict()

        assert item_dict["id"] == item.id
        assert item_dict["description"] == "Test Item"
        assert item_dict["checked"] == True
        assert "created_at" in item_dict
        assert "updated_at" in item_dict
        assert isinstance(item_dict["created_at"], str)
        assert isinstance(item_dict["updated_at"], str)

    def test_item_to_dict_with_none_timestamps(self, db_session):
        """Test to_dict handles None timestamps gracefully"""
        item = Item(description="Test", checked=False)
        # Don't commit yet to avoid auto-setting timestamps in some cases
        item_dict = item.to_dict()

        # Should handle None timestamps without error
        if item.created_at:
            assert isinstance(item_dict["created_at"], str)
        if item.updated_at:
            assert isinstance(item_dict["updated_at"], str)

    def test_item_query_by_description(self, db_session):
        """Test querying items by description"""
        item1 = Item(description="Apples", checked=False)
        item2 = Item(description="Oranges", checked=False)
        db_session.add_all([item1, item2])
        db_session.commit()

        found = db_session.query(Item).filter(Item.description == "Apples").first()
        assert found is not None
        assert found.description == "Apples"

    def test_item_query_by_checked_status(self, db_session):
        """Test querying items by checked status"""
        item1 = Item(description="Checked", checked=True)
        item2 = Item(description="Unchecked", checked=False)
        item3 = Item(description="Also Checked", checked=True)
        db_session.add_all([item1, item2, item3])
        db_session.commit()

        checked_items = db_session.query(Item).filter(Item.checked == True).all()
        assert len(checked_items) == 2
        assert all(item.checked for item in checked_items)

    def test_item_update(self, db_session):
        """Test updating an item"""
        item = Item(description="Original", checked=False)
        db_session.add(item)
        db_session.commit()
        item_id = item.id

        # Update the item
        item.description = "Updated"
        item.checked = True
        db_session.commit()

        # Query and verify
        updated = db_session.query(Item).filter(Item.id == item_id).first()
        assert updated.description == "Updated"
        assert updated.checked == True

    def test_item_delete(self, db_session):
        """Test deleting an item"""
        item = Item(description="To Delete", checked=False)
        db_session.add(item)
        db_session.commit()
        item_id = item.id

        # Delete the item
        item_to_delete = db_session.query(Item).filter(Item.id == item_id).first()
        db_session.delete(item_to_delete)
        db_session.commit()

        # Verify deletion
        remaining = db_session.query(Item).filter(Item.id == item_id).first()
        assert remaining is None

    def test_item_ilike_query(self, db_session):
        """Test case-insensitive search using ilike"""
        item1 = Item(description="APPLE", checked=False)
        item2 = Item(description="apple", checked=False)
        item3 = Item(description="Banana", checked=False)
        db_session.add_all([item1, item2, item3])
        db_session.commit()

        # Search case-insensitively
        found = db_session.query(Item).filter(
            Item.description.ilike("%apple%")
        ).all()
        assert len(found) == 2

    def test_multiple_items(self, db_session):
        """Test creating multiple items"""
        items = [
            Item(description=f"Item {i}", checked=i % 2 == 0)
            for i in range(5)
        ]
        db_session.add_all(items)
        db_session.commit()

        all_items = db_session.query(Item).all()
        assert len(all_items) == 5
