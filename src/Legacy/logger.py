import os
import traceback
from datetime import datetime

log_dir = "logs"
os.makedirs(log_dir, exist_ok=True)

def write_to_log(message, level="INFO"):
    """Write directly to log file"""
    timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S,%f')[:-3]
    with open(f'{log_dir}/logs.log', 'a', encoding='utf-8') as f:
        f.write(f"{timestamp} - api_logger - {level} - {message}\n")
        f.flush()

def log_info(message):
    """Log info message"""
    write_to_log(message, "INFO")
    print(f"LOG: {message}")  # Also print to console for debugging

def log_error(message, error=None):
    """Log error message with optional stack trace"""
    error_message = message
    if error:
        # Add stack trace to error message
        stack_trace = traceback.format_exc()
        error_message = f"{message}\nStack Trace:\n{stack_trace}"
    
    write_to_log(error_message, "ERROR")
    print(f"ERROR: {error_message}")

def log_warning(message):
    """Log warning message"""
    write_to_log(message, "WARNING")
    print(f"WARNING: {message}")

# Create a simple logger that writes directly to file
logger = type('Logger', (), {
    'info': lambda self, msg: log_info(msg),
    'error': lambda self, msg, err=None: log_error(msg, err),
    'warning': lambda self, msg: log_warning(msg),
})()