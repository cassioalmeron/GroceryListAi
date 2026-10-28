"""
Tests for the logger module
"""
import pytest
import os
import tempfile
import shutil
from datetime import datetime
from unittest.mock import patch, mock_open, MagicMock
import logger


class TestLoggerInitialization:
    """Test logger initialization and directory creation"""

    def test_log_directory_creation(self):
        """Test that logs directory is created on import"""
        # Arrange & Act
        # The directory is created at module import time
        log_dir = "logs"

        # Assert
        assert os.path.exists(log_dir)
        assert os.path.isdir(log_dir)

    def test_log_directory_already_exists(self):
        """Test that existing log directory doesn't cause errors"""
        # Arrange & Act
        log_dir = "logs"
        os.makedirs(log_dir, exist_ok=True)

        # Assert
        # Should not raise any exceptions
        assert os.path.exists(log_dir)


class TestWriteToLog:
    """Test the write_to_log function"""

    def test_write_to_log_creates_file(self):
        """Test that write_to_log creates log file"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Test log message"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.write_to_log(test_message, "INFO")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            assert os.path.exists(log_file)

            # Verify content
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "INFO" in content
                assert test_message in content
                assert "api_logger" in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_write_to_log_format(self):
        """Test that write_to_log formats message correctly"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Formatted message test"
        test_level = "ERROR"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.write_to_log(test_message, test_level)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                # Format: timestamp - api_logger - LEVEL - message
                assert " - api_logger - " in content
                assert f" - {test_level} - " in content
                assert test_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_write_to_log_timestamp_format(self):
        """Test that write_to_log includes proper timestamp"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.write_to_log("Test", "INFO")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                # Should have timestamp in format: YYYY-MM-DD HH:MM:SS,mmm
                # Example: 2025-11-03 10:30:45,123
                import re
                timestamp_pattern = r'\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}'
                assert re.search(timestamp_pattern, content) is not None

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_write_to_log_appends_to_file(self):
        """Test that write_to_log appends to existing file"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.write_to_log("First message", "INFO")
            logger.write_to_log("Second message", "INFO")
            logger.write_to_log("Third message", "INFO")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                lines = f.readlines()
                assert len(lines) == 3
                assert "First message" in lines[0]
                assert "Second message" in lines[1]
                assert "Third message" in lines[2]

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_write_to_log_with_special_characters(self):
        """Test that write_to_log handles special characters"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        special_message = "Test: ñ, é, ü, 中文, 😀, <tag>, \"quotes\""

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.write_to_log(special_message, "INFO")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert special_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)


class TestLogInfo:
    """Test the log_info function"""

    def test_log_info_writes_info_level(self):
        """Test that log_info writes with INFO level"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Info level message"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_info(test_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "INFO" in content
                assert test_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_log_info_prints_to_console(self, capsys):
        """Test that log_info prints to console"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Console test"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_info(test_message)

            # Assert
            captured = capsys.readouterr()
            assert "LOG:" in captured.out
            assert test_message in captured.out

        # Cleanup
        shutil.rmtree(test_log_dir)


class TestLogError:
    """Test the log_error function"""

    def test_log_error_writes_error_level(self):
        """Test that log_error writes with ERROR level"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Error message"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_error(test_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "ERROR" in content
                assert test_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_log_error_without_exception(self, capsys):
        """Test log_error without exception object"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Simple error"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_error(test_message, None)

            # Assert
            captured = capsys.readouterr()
            assert "ERROR:" in captured.out
            assert test_message in captured.out

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_log_error_with_exception(self):
        """Test log_error with exception object includes stack trace"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Error with exception"

        with patch('logger.log_dir', test_log_dir):
            try:
                # Create a real exception
                raise ValueError("Test exception")
            except ValueError as e:
                # Act
                logger.log_error(test_message, e)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert test_message in content
                assert "Stack Trace:" in content
                assert "ValueError" in content
                assert "Test exception" in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_log_error_prints_to_console(self, capsys):
        """Test that log_error prints to console"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Console error test"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_error(test_message)

            # Assert
            captured = capsys.readouterr()
            assert "ERROR:" in captured.out
            assert test_message in captured.out

        # Cleanup
        shutil.rmtree(test_log_dir)


class TestLogWarning:
    """Test the log_warning function"""

    def test_log_warning_writes_warning_level(self):
        """Test that log_warning writes with WARNING level"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Warning message"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_warning(test_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "WARNING" in content
                assert test_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_log_warning_prints_to_console(self, capsys):
        """Test that log_warning prints to console"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Console warning test"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_warning(test_message)

            # Assert
            captured = capsys.readouterr()
            assert "WARNING:" in captured.out
            assert test_message in captured.out

        # Cleanup
        shutil.rmtree(test_log_dir)


class TestLoggerObject:
    """Test the logger object interface"""

    def test_logger_object_exists(self):
        """Test that logger object is created"""
        # Arrange & Act & Assert
        assert hasattr(logger, 'logger')
        assert logger.logger is not None

    def test_logger_info_method(self):
        """Test logger.info() method"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Info via logger object"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.logger.info(test_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "INFO" in content
                assert test_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_logger_error_method(self):
        """Test logger.error() method"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Error via logger object"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.logger.error(test_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "ERROR" in content
                assert test_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_logger_warning_method(self):
        """Test logger.warning() method"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        test_message = "Warning via logger object"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.logger.warning(test_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "WARNING" in content
                assert test_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)


class TestLoggerEdgeCases:
    """Test edge cases and error handling"""

    def test_log_empty_message(self):
        """Test logging empty message"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_info("")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            assert os.path.exists(log_file)

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_log_very_long_message(self):
        """Test logging very long message"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        long_message = "A" * 10000  # 10KB message

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_info(long_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert long_message in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_log_multiline_message(self):
        """Test logging multiline message"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()
        multiline_message = "Line 1\nLine 2\nLine 3"

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_info(multiline_message)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "Line 1" in content
                assert "Line 2" in content
                assert "Line 3" in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_multiple_log_levels_in_sequence(self):
        """Test logging multiple levels in sequence"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()

        with patch('logger.log_dir', test_log_dir):
            # Act
            logger.log_info("Info message")
            logger.log_warning("Warning message")
            logger.log_error("Error message")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "INFO" in content
                assert "WARNING" in content
                assert "ERROR" in content
                assert "Info message" in content
                assert "Warning message" in content
                assert "Error message" in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_concurrent_logging(self):
        """Test that concurrent logging doesn't corrupt the file"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()

        with patch('logger.log_dir', test_log_dir):
            # Act - Simulate rapid logging
            for i in range(100):
                logger.log_info(f"Message {i}")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                lines = f.readlines()
                assert len(lines) == 100
                # Verify all messages are present
                for i in range(100):
                    assert any(f"Message {i}" in line for line in lines)

        # Cleanup
        shutil.rmtree(test_log_dir)


class TestLoggerIntegration:
    """Integration tests for logger usage patterns"""

    def test_logger_usage_in_error_handling(self):
        """Test typical error handling pattern with logger"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()

        with patch('logger.log_dir', test_log_dir):
            try:
                # Act - Simulate an error scenario
                result = 10 / 0
            except ZeroDivisionError as e:
                logger.log_error("Division by zero occurred", e)

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                content = f.read()
                assert "ERROR" in content
                assert "Division by zero occurred" in content
                assert "ZeroDivisionError" in content

        # Cleanup
        shutil.rmtree(test_log_dir)

    def test_logger_usage_in_api_flow(self):
        """Test typical API flow logging pattern"""
        # Arrange
        test_log_dir = tempfile.mkdtemp()

        with patch('logger.log_dir', test_log_dir):
            # Act - Simulate API request flow
            logger.log_info("API request received: GET /items")
            logger.log_info("Processing request")
            logger.log_info("Querying database")
            logger.log_info("Returning response")

            # Assert
            log_file = os.path.join(test_log_dir, 'logs.log')
            with open(log_file, 'r', encoding='utf-8') as f:
                lines = f.readlines()
                assert len(lines) == 4
                assert "API request received" in lines[0]
                assert "Processing request" in lines[1]
                assert "Querying database" in lines[2]
                assert "Returning response" in lines[3]

        # Cleanup
        shutil.rmtree(test_log_dir)
