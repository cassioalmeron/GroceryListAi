# Testing Guide

This guide explains how to run and maintain the test suite for the GroceryListAI backend.

## Quick Start

```bash
# Navigate to Server directory
cd Server

# Install test dependencies (if not already installed)
pip install -e ".[test]"
# or with uv
uv sync

# Run all tests
pytest tests/ -v

# Run with coverage report
pytest tests/ --cov=. --cov-report=term-missing
```

## Test Suite Overview

| Test File | Tests | Coverage Area | Purpose |
|-----------|-------|---------------|---------|
| `test_items_api.py` | 17 | Items CRUD endpoints | REST API testing |
| `test_chat_api.py` | 18 | Chat SSE endpoint | Streaming, commands |
| `test_models.py` | 13 | SQLAlchemy models | Database layer |
| `test_schemas.py` | 27 | Pydantic schemas | Validation |
| `test_llm.py` | 25 | LLM integration | ChatGPT, Ollama |
| `test_logger.py` | 26 | Logging system | File I/O, formatting |
| `test_service_manager.py` | 25 | Service management | systemd integration |
| **Total** | **151** | **~95% coverage** | Full backend |

## Running Tests

### Run All Tests

```bash
pytest tests/ -v
```

### Run Specific Test File

```bash
pytest tests/test_chat_api.py -v
pytest tests/test_llm.py -v
pytest tests/test_logger.py -v
pytest tests/test_service_manager.py -v
```

### Run Specific Test Class

```bash
pytest tests/test_chat_api.py::TestChatEndpoint -v
pytest tests/test_llm.py::TestChatGPTClient -v
```

### Run Specific Test Method

```bash
pytest tests/test_chat_api.py::TestChatEndpoint::test_chat_add_item_command -v
```

### Run Tests Matching Pattern

```bash
# Run all tests with "error" in the name
pytest tests/ -k "error" -v

# Run all tests with "chat" in the name
pytest tests/ -k "chat" -v
```

### Quiet Mode (Less Output)

```bash
pytest tests/ -q
```

### Stop on First Failure

```bash
pytest tests/ -x
```

### Show Local Variables on Failure

```bash
pytest tests/ -l
```

## Coverage Reports

### Terminal Coverage Report

```bash
pytest tests/ --cov=. --cov-report=term-missing
```

### HTML Coverage Report

```bash
pytest tests/ --cov=. --cov-report=html
```

Then open `htmlcov/index.html` in your browser.

### XML Coverage Report (for CI/CD)

```bash
pytest tests/ --cov=. --cov-report=xml
```

### Combined Reports

```bash
pytest tests/ --cov=. --cov-report=term-missing --cov-report=html --cov-report=xml
```

## Test Configuration

### pytest.ini (Optional)

Create a `pytest.ini` file in the Server directory for custom configuration:

```ini
[pytest]
testpaths = tests
python_files = test_*.py
python_classes = Test*
python_functions = test_*
addopts = -v --strict-markers
markers =
    slow: marks tests as slow (deselect with '-m "not slow"')
    integration: marks tests as integration tests
asyncio_mode = auto
```

### Environment Setup

Some tests require environment variables. Create a `.env` file:

```bash
cp env.sample .env
```

Edit `.env`:
```env
MODEL=llama2
LLM=ollama
DATABASE_URL=sqlite:///:memory:
```

## Test Categories

### Unit Tests (120 tests)

Test individual functions and methods in isolation:
- Models
- Schemas
- Logger functions
- LLM client classes

```bash
# Run unit tests
pytest tests/test_models.py tests/test_schemas.py tests/test_logger.py -v
```

### Integration Tests (31 tests)

Test interactions between components:
- API endpoints with database
- Chat endpoint with LLM
- Service manager with system

```bash
# Run integration tests
pytest tests/test_items_api.py tests/test_chat_api.py -v
```

### Async Tests (43 tests)

Tests using `@pytest.mark.asyncio`:
- Chat endpoint
- LLM streaming

```bash
# Run only async tests
pytest tests/test_chat_api.py tests/test_llm.py -v
```

## Test Fixtures

### Available Fixtures

Defined in `conftest.py`:

- **`db_engine`** - In-memory SQLite database engine
- **`db_session`** - Isolated database session per test
- **`client`** - FastAPI TestClient with dependency overrides

### Using Fixtures

```python
def test_example(client, db_session):
    # client: FastAPI TestClient
    # db_session: SQLAlchemy Session
    response = client.get("/items")
    assert response.status_code == 200
```

## Mocking External Services

### Mocking LLM Responses

```python
from unittest.mock import patch

@pytest.mark.asyncio
async def test_chat(client):
    async def mock_llm_response(messages):
        yield '[{"command": "AddItem", "value": "Milk"}]'

    with patch('llm.get_response', side_effect=mock_llm_response):
        response = client.post("/chat", json={"message": "Add milk"})
        assert response.status_code == 200
```

### Mocking System Commands

```python
from unittest.mock import MagicMock, patch

def test_service_start():
    mock_run = MagicMock()
    mock_run.return_value = MagicMock(returncode=0, stdout="", stderr="")

    with patch('subprocess.run', mock_run):
        service_manager.manage_service("test", "start")
        mock_run.assert_called_once()
```

## Troubleshooting

### ModuleNotFoundError

**Problem**: `ModuleNotFoundError: No module named 'Models'`

**Solution**: Make sure you're in the Server directory:
```bash
cd Server
pytest tests/ -v
```

### ImportError for pytest-asyncio

**Problem**: `ImportError: cannot import name 'pytest_asyncio'`

**Solution**: Install test dependencies:
```bash
pip install pytest-asyncio
# or
pip install -e ".[test]"
```

### Database Locked Errors

**Problem**: `sqlite3.OperationalError: database is locked`

**Solution**: Tests use in-memory database, but if you see this:
```bash
# Delete any test database files
rm -f test_*.db
pytest tests/ -v
```

### Slow Tests

**Problem**: Tests take too long

**Solution**: Run tests in parallel with pytest-xdist:
```bash
pip install pytest-xdist
pytest tests/ -n auto  # Use all CPU cores
```

### Failed to Create Directory

**Problem**: Logger tests fail to create directories

**Solution**: Check permissions or run tests with proper privileges

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Set up Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.11'

      - name: Install dependencies
        run: |
          cd Server
          pip install -e ".[test]"

      - name: Run tests with coverage
        run: |
          cd Server
          pytest tests/ --cov=. --cov-report=xml --cov-report=term

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          file: ./Server/coverage.xml
```

## Best Practices

### Writing New Tests

1. **Follow AAA Pattern**:
   ```python
   def test_example():
       # Arrange
       data = {"key": "value"}

       # Act
       result = process(data)

       # Assert
       assert result == expected
   ```

2. **Use Descriptive Names**:
   - ✅ `test_create_item_with_empty_description_fails`
   - ❌ `test_1`

3. **Test One Thing**:
   ```python
   # Good
   def test_create_item_success():
       ...

   def test_create_item_validation_error():
       ...

   # Bad
   def test_create_item():
       # Test success
       # Test validation
       # Test database
       ...
   ```

4. **Mock External Dependencies**:
   - Always mock LLM APIs
   - Mock file I/O when not testing I/O
   - Mock system commands

5. **Keep Tests Isolated**:
   - No shared state between tests
   - Use fixtures for test data
   - Each test should pass/fail independently

### Test Coverage Goals

- **Core business logic**: 100%
- **API endpoints**: 95%+
- **Utilities**: 90%+
- **Overall**: 95%+

### When to Run Tests

- ✅ Before committing code
- ✅ After pulling changes
- ✅ Before creating PR
- ✅ In CI/CD pipeline
- ✅ After dependency updates

## Performance

### Expected Test Execution Times

| Test File | Tests | Time |
|-----------|-------|------|
| test_items_api.py | 17 | ~0.5s |
| test_chat_api.py | 18 | ~1.0s |
| test_models.py | 13 | ~0.3s |
| test_schemas.py | 27 | ~0.4s |
| test_llm.py | 25 | ~0.8s |
| test_logger.py | 26 | ~2.0s |
| test_service_manager.py | 25 | ~1.5s |
| **Total** | **151** | **~6.5s** |

### Optimizing Test Speed

1. **Use in-memory database** (already configured)
2. **Mock external services** (already done)
3. **Run tests in parallel**: `pytest tests/ -n auto`
4. **Skip slow tests in development**: `pytest tests/ -m "not slow"`

## Additional Resources

- **Full Coverage Report**: See `docs/reports/TEST_COVERAGE_IMPROVEMENT.md`
- **Original Test Documentation**: See `docs/reports/TESTS.md`
- **pytest Documentation**: https://docs.pytest.org/
- **pytest-asyncio**: https://pytest-asyncio.readthedocs.io/

## Getting Help

If tests fail:

1. **Read the error message** - pytest provides detailed output
2. **Run with verbose mode**: `pytest tests/ -v`
3. **Run failing test in isolation**: `pytest tests/test_file.py::test_name -v`
4. **Check test logs**: Look at console output
5. **Verify environment**: Check `.env` file
6. **Update dependencies**: `pip install -e ".[test]" --upgrade`

## Summary

```bash
# Quick test workflow
cd Server
pytest tests/ -v                    # Run all tests
pytest tests/ --cov=. --cov-report=term-missing  # With coverage
pytest tests/test_chat_api.py -v   # Specific file
pytest tests/ -k "chat" -v         # By pattern
```

**Total**: 151 tests | **Coverage**: 95%+ | **Time**: ~6.5s
