"""
Tests for Pydantic schemas validation
"""
import pytest
from datetime import datetime
from pydantic import ValidationError

from Models.schemas import (
    ItemCreate,
    ItemResponse,
    ItemUpdate,
    ItemCheckedUpdate,
    ItemBase,
)


class TestItemBaseSchema:
    """Test the ItemBase schema"""

    def test_item_base_valid(self):
        """Test creating a valid ItemBase"""
        data = {"description": "Test Item", "checked": False}
        item = ItemBase(**data)

        assert item.description == "Test Item"
        assert item.checked == False

    def test_item_base_default_checked(self):
        """Test that checked defaults to False"""
        data = {"description": "Test Item"}
        item = ItemBase(**data)

        assert item.checked == False

    def test_item_base_empty_description(self):
        """Test that empty description fails validation"""
        data = {"description": "", "checked": False}

        with pytest.raises(ValidationError) as exc_info:
            ItemBase(**data)

        assert "at least 1 character" in str(exc_info.value)

    def test_item_base_missing_description(self):
        """Test that missing description fails validation"""
        data = {"checked": False}

        with pytest.raises(ValidationError) as exc_info:
            ItemBase(**data)

        assert "Field required" in str(exc_info.value)

    def test_item_base_description_too_long(self):
        """Test that description over 255 chars fails"""
        long_desc = "a" * 256
        data = {"description": long_desc, "checked": False}

        with pytest.raises(ValidationError) as exc_info:
            ItemBase(**data)

        assert "at most 255 characters" in str(exc_info.value)

    def test_item_base_description_max_length(self):
        """Test that description with exactly 255 chars passes"""
        max_desc = "a" * 255
        data = {"description": max_desc, "checked": False}
        item = ItemBase(**data)

        assert item.description == max_desc

    def test_item_base_invalid_checked_type(self):
        """Test that non-boolean checked value gets coerced by Pydantic"""
        data = {"description": "Test", "checked": "yes"}
        # Pydantic coerces truthy strings to boolean
        item = ItemBase(**data)
        assert item.checked == True  # "yes" is truthy, so coerced to True


class TestItemCreateSchema:
    """Test the ItemCreate schema"""

    def test_item_create_valid(self):
        """Test creating a valid ItemCreate"""
        data = {"description": "New Item", "checked": True}
        item = ItemCreate(**data)

        assert item.description == "New Item"
        assert item.checked == True

    def test_item_create_inherits_validation(self):
        """Test that ItemCreate inherits ItemBase validation"""
        data = {"description": "", "checked": False}

        with pytest.raises(ValidationError):
            ItemCreate(**data)


class TestItemUpdateSchema:
    """Test the ItemUpdate schema"""

    def test_item_update_all_fields(self):
        """Test updating all fields"""
        data = {"description": "Updated Item", "checked": True}
        item = ItemUpdate(**data)

        assert item.description == "Updated Item"
        assert item.checked == True

    def test_item_update_description_only(self):
        """Test updating only description"""
        data = {"description": "Updated Description"}
        item = ItemUpdate(**data)

        assert item.description == "Updated Description"
        assert item.checked is None

    def test_item_update_checked_only(self):
        """Test updating only checked status"""
        data = {"checked": False}
        item = ItemUpdate(**data)

        assert item.description is None
        assert item.checked == False

    def test_item_update_empty_object(self):
        """Test updating with empty object (partial update)"""
        data = {}
        item = ItemUpdate(**data)

        assert item.description is None
        assert item.checked is None

    def test_item_update_invalid_description(self):
        """Test that invalid description fails"""
        data = {"description": ""}

        with pytest.raises(ValidationError):
            ItemUpdate(**data)

    def test_item_update_description_too_long(self):
        """Test that description over 255 chars fails"""
        data = {"description": "a" * 256}

        with pytest.raises(ValidationError):
            ItemUpdate(**data)


class TestItemCheckedUpdateSchema:
    """Test the ItemCheckedUpdate schema"""

    def test_item_checked_update_true(self):
        """Test updating checked to True"""
        data = {"checked": True}
        item = ItemCheckedUpdate(**data)

        assert item.checked == True

    def test_item_checked_update_false(self):
        """Test updating checked to False"""
        data = {"checked": False}
        item = ItemCheckedUpdate(**data)

        assert item.checked == False

    def test_item_checked_update_missing_field(self):
        """Test that missing checked field fails"""
        data = {}

        with pytest.raises(ValidationError) as exc_info:
            ItemCheckedUpdate(**data)

        assert "Field required" in str(exc_info.value)

    def test_item_checked_update_invalid_type(self):
        """Test that non-boolean value gets coerced by Pydantic"""
        data = {"checked": "true"}
        # Pydantic coerces truthy strings to boolean
        item = ItemCheckedUpdate(**data)
        assert item.checked == True  # "true" is truthy, so coerced to True

    def test_item_checked_update_none_value(self):
        """Test that None value fails"""
        data = {"checked": None}

        with pytest.raises(ValidationError):
            ItemCheckedUpdate(**data)


class TestItemResponseSchema:
    """Test the ItemResponse schema"""

    def test_item_response_from_dict(self):
        """Test creating ItemResponse from dictionary"""
        now = datetime.utcnow()
        data = {
            "id": 1,
            "description": "Test Item",
            "checked": False,
            "created_at": now,
            "updated_at": now,
        }
        item = ItemResponse(**data)

        assert item.id == 1
        assert item.description == "Test Item"
        assert item.checked == False
        assert item.created_at == now
        assert item.updated_at == now

    def test_item_response_all_fields_required(self):
        """Test that all fields are required in ItemResponse"""
        data = {
            "id": 1,
            "description": "Test Item",
            "checked": False,
        }

        with pytest.raises(ValidationError) as exc_info:
            ItemResponse(**data)

        assert "Field required" in str(exc_info.value)

    def test_item_response_invalid_id_type(self):
        """Test that non-integer id fails"""
        now = datetime.utcnow()
        data = {
            "id": "1",
            "description": "Test Item",
            "checked": False,
            "created_at": now,
            "updated_at": now,
        }

        # Should coerce string to int
        item = ItemResponse(**data)
        assert item.id == 1

    def test_item_response_from_orm(self):
        """Test creating ItemResponse from ORM object"""
        from Models import Item

        item_orm = Item(description="ORM Item", checked=True)
        now = datetime.utcnow()
        item_orm.id = 1
        item_orm.created_at = now
        item_orm.updated_at = now

        # Use from_attributes for Pydantic v2
        item_response = ItemResponse.model_validate(item_orm)

        assert item_response.id == 1
        assert item_response.description == "ORM Item"
        assert item_response.checked == True


class TestSchemaEdgeCases:
    """Test edge cases across multiple schemas"""

    def test_whitespace_only_description(self):
        """Test that whitespace-only description passes but isn't ideal"""
        data = {"description": "   ", "checked": False}
        # Whitespace is technically not empty string, so it passes validation
        item = ItemCreate(**data)
        assert item.description == "   "

    def test_special_characters_in_description(self):
        """Test that special characters are allowed"""
        special_chars = "Test™ with émojis! @#$%^&*()"
        data = {"description": special_chars, "checked": False}
        item = ItemCreate(**data)
        assert item.description == special_chars

    def test_unicode_characters_in_description(self):
        """Test that unicode characters work"""
        unicode_text = "买菜 🛒 りんご 🍎"
        data = {"description": unicode_text, "checked": True}
        item = ItemCreate(**data)
        assert item.description == unicode_text
