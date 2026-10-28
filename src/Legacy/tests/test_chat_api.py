"""
Tests for the chat API endpoint with SSE streaming
"""
import pytest
import json
from unittest.mock import AsyncMock, patch, MagicMock
from Models import Item


class TestChatEndpoint:
    """Test the POST /chat endpoint with SSE streaming"""

    @pytest.mark.asyncio
    async def test_chat_endpoint_with_single_message(self, client, db_session):
        """Test chat endpoint with single message format"""
        # Arrange
        payload = {"message": "Add milk to the list"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "AddItem", "value": "Milk"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            assert response.headers["content-type"] == "text/event-stream; charset=utf-8"

            # Verify item was added to database
            items = db_session.query(Item).all()
            assert len(items) == 1
            assert items[0].description == "Milk"
            assert items[0].checked == False

    @pytest.mark.asyncio
    async def test_chat_endpoint_with_messages_array(self, client, db_session):
        """Test chat endpoint with messages array format"""
        # Arrange
        payload = {
            "messages": [
                {"role": "user", "content": "Add bread"},
                {"role": "assistant", "content": "Added bread"},
                {"role": "user", "content": "Add butter"}
            ]
        }

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "AddItem", "value": "Butter"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            assert response.headers["content-type"] == "text/event-stream; charset=utf-8"

    @pytest.mark.asyncio
    async def test_chat_add_item_command(self, client, db_session):
        """Test chat endpoint executing AddItem command"""
        # Arrange
        payload = {"message": "Add apples and oranges"}

        # Mock the LLM response with multiple items
        async def mock_llm_response(messages):
            yield '[{"command": "AddItem", "value": "Apples"}, '
            yield '{"command": "AddItem", "value": "Oranges"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify items were added
            items = db_session.query(Item).all()
            assert len(items) == 2
            descriptions = [item.description for item in items]
            assert "Apples" in descriptions
            assert "Oranges" in descriptions

    @pytest.mark.asyncio
    async def test_chat_remove_item_command(self, client, db_session):
        """Test chat endpoint executing RemoveItem command"""
        # Arrange - Create an item first
        item = Item(description="Milk", checked=False)
        db_session.add(item)
        db_session.commit()

        payload = {"message": "Remove milk"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "RemoveItem", "value": "Milk"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify item was removed
            items = db_session.query(Item).all()
            assert len(items) == 0

    @pytest.mark.asyncio
    async def test_chat_remove_item_partial_match(self, client, db_session):
        """Test RemoveItem command with partial description match"""
        # Arrange - Create an item
        item = Item(description="Whole Milk", checked=False)
        db_session.add(item)
        db_session.commit()

        payload = {"message": "Remove milk"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "RemoveItem", "value": "milk"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify item was removed (case-insensitive match)
            items = db_session.query(Item).all()
            assert len(items) == 0

    @pytest.mark.asyncio
    async def test_chat_remove_non_existing_item(self, client, db_session):
        """Test RemoveItem command for non-existing item"""
        # Arrange
        payload = {"message": "Remove nonexistent"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "RemoveItem", "value": "nonexistent"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            # Should not crash, just log item not found

    @pytest.mark.asyncio
    async def test_chat_check_item_command(self, client, db_session):
        """Test chat endpoint executing CheckItem command"""
        # Arrange - Create an unchecked item
        item = Item(description="Bread", checked=False)
        db_session.add(item)
        db_session.commit()

        payload = {"message": "Mark bread as checked"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "CheckItem", "value": "Bread"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify item was checked
            db_session.expire_all()  # Refresh from database
            updated_item = db_session.query(Item).filter(Item.description.ilike("%bread%")).first()
            assert updated_item is not None
            assert updated_item.checked == True

    @pytest.mark.asyncio
    async def test_chat_uncheck_item_command(self, client, db_session):
        """Test chat endpoint executing UncheckItem command"""
        # Arrange - Create a checked item
        item = Item(description="Eggs", checked=True)
        db_session.add(item)
        db_session.commit()

        payload = {"message": "Uncheck eggs"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "UncheckItem", "value": "Eggs"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify item was unchecked
            db_session.expire_all()
            updated_item = db_session.query(Item).filter(Item.description.ilike("%eggs%")).first()
            assert updated_item is not None
            assert updated_item.checked == False

    @pytest.mark.asyncio
    async def test_chat_check_non_existing_item(self, client, db_session):
        """Test CheckItem command for non-existing item"""
        # Arrange
        payload = {"message": "Check nonexistent"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[{"command": "CheckItem", "value": "nonexistent"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            # Should not crash, just log item not found

    @pytest.mark.asyncio
    async def test_chat_empty_command_response(self, client, db_session):
        """Test chat endpoint with empty command array response"""
        # Arrange
        payload = {"message": "What's the weather?"}

        # Mock the LLM response with empty array (out of scope)
        async def mock_llm_response(messages):
            yield '[]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            # No items should be added
            items = db_session.query(Item).all()
            assert len(items) == 0

    @pytest.mark.asyncio
    async def test_chat_multiple_commands_in_sequence(self, client, db_session):
        """Test chat endpoint with multiple commands in one response"""
        # Arrange
        payload = {"message": "Add milk, remove bread, and check eggs"}

        # Create an item to be removed
        item1 = Item(description="Bread", checked=False)
        item2 = Item(description="Eggs", checked=False)
        db_session.add(item1)
        db_session.add(item2)
        db_session.commit()

        # Mock the LLM response with multiple commands
        async def mock_llm_response(messages):
            yield '[{"command": "AddItem", "value": "Milk"}, '
            yield '{"command": "RemoveItem", "value": "Bread"}, '
            yield '{"command": "CheckItem", "value": "Eggs"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify all commands were executed
            db_session.expire_all()
            items = db_session.query(Item).all()
            assert len(items) == 2  # Milk added, Bread removed, Eggs remains

            descriptions = {item.description: item for item in items}
            assert "Milk" in descriptions
            assert "Bread" not in descriptions
            assert "Eggs" in descriptions
            assert descriptions["Eggs"].checked == True

    @pytest.mark.asyncio
    async def test_chat_with_json_wrapped_in_code_blocks(self, client, db_session):
        """Test chat endpoint handling JSON wrapped in markdown code blocks"""
        # Arrange
        payload = {"message": "Add cheese"}

        # Mock the LLM response with markdown code blocks
        async def mock_llm_response(messages):
            yield '```json\n'
            yield '[{"command": "AddItem", "value": "Cheese"}]\n'
            yield '```'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify item was added despite code blocks
            items = db_session.query(Item).all()
            assert len(items) == 1
            assert items[0].description == "Cheese"

    @pytest.mark.asyncio
    async def test_chat_with_invalid_json_response(self, client, db_session):
        """Test chat endpoint handling invalid JSON response from LLM"""
        # Arrange
        payload = {"message": "Add something"}

        # Mock the LLM response with invalid JSON
        async def mock_llm_response(messages):
            yield 'This is not valid JSON'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            # Should handle gracefully without crashing
            items = db_session.query(Item).all()
            assert len(items) == 0

    @pytest.mark.asyncio
    async def test_chat_streaming_response_format(self, client, db_session):
        """Test that chat endpoint returns proper SSE headers"""
        # Arrange
        payload = {"message": "Test"}

        # Mock the LLM response
        async def mock_llm_response(messages):
            yield '[]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            assert "text/event-stream" in response.headers["content-type"]
            assert response.headers.get("Cache-Control") == "no-cache"
            assert response.headers.get("Connection") == "keep-alive"
            assert response.headers.get("X-Accel-Buffering") == "no"

    @pytest.mark.asyncio
    async def test_chat_case_insensitive_item_matching(self, client, db_session):
        """Test that item commands match case-insensitively"""
        # Arrange - Create item with mixed case
        item = Item(description="Whole Milk", checked=False)
        db_session.add(item)
        db_session.commit()

        payload = {"message": "Check whole milk"}

        # Mock the LLM response with different case
        async def mock_llm_response(messages):
            yield '[{"command": "CheckItem", "value": "WHOLE MILK"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200

            # Verify item was found and checked
            db_session.expire_all()
            updated_item = db_session.query(Item).filter(
                Item.description.ilike("%whole milk%")
            ).first()
            assert updated_item is not None
            assert updated_item.checked == True

    @pytest.mark.asyncio
    async def test_chat_with_command_without_value(self, client, db_session):
        """Test chat endpoint handling command without value field"""
        # Arrange
        payload = {"message": "Add item"}

        # Mock the LLM response with missing value
        async def mock_llm_response(messages):
            yield '[{"command": "AddItem"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            # Should handle gracefully - item with empty description
            items = db_session.query(Item).all()
            # Might add item with empty value or skip it

    @pytest.mark.asyncio
    async def test_chat_with_unknown_command(self, client, db_session):
        """Test chat endpoint handling unknown command type"""
        # Arrange
        payload = {"message": "Do something"}

        # Mock the LLM response with unknown command
        async def mock_llm_response(messages):
            yield '[{"command": "UnknownCommand", "value": "Something"}]'

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act
            response = client.post("/chat", json=payload)

            # Assert
            assert response.status_code == 200
            # Should handle gracefully - just ignore unknown command
            items = db_session.query(Item).all()
            assert len(items) == 0

    @pytest.mark.asyncio
    async def test_chat_with_llm_exception(self, client, db_session):
        """Test chat endpoint handling LLM exception"""
        # Arrange
        payload = {"message": "Test"}

        # Mock the LLM to raise an exception
        async def mock_llm_response(messages):
            raise Exception("LLM API error")
            yield  # Make it a generator

        with patch('llm.get_response', side_effect=mock_llm_response):
            # Act & Assert
            # The endpoint should handle the exception gracefully
            # In the actual implementation, the exception is caught in llm.get_response
            response = client.post("/chat", json=payload)
            assert response.status_code == 200
