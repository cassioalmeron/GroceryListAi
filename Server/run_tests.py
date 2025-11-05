"""
Simple test runner script that can be executed directly
"""
import sys
import subprocess

# Run pytest with arguments
result = subprocess.run(
    [sys.executable, "-m", "pytest", "tests/", "-v"],
    cwd=r"C:\Temp\Github\GroceryListAi\Server"
)

sys.exit(result.returncode)
