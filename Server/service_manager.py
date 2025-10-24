import os
import sys
import subprocess

def manage_service(service_name, action, app_path=None):
    service_name = service_name.replace(' ', '-')
    service_file = f"/etc/systemd/system/{service_name}.service"
    
    def run_cmd(cmd, use_sudo=False):
        if use_sudo:
            cmd = f"sudo {cmd}"
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
        return result.returncode == 0, result.stdout, result.stderr
    
    if action == "install" and app_path:
        script_dir = os.path.dirname(app_path)
        # Detect virtual environment
        venv_python = os.path.join(script_dir, 'venv', 'bin', 'python')
        
        # Check if virtual environment exists
        if os.path.exists(venv_python):
            python_path = venv_python
            env_path = os.path.join(script_dir, 'venv', 'bin')
            print(f"Using virtual environment: {python_path}")
        else:
            python_path = '/usr/bin/python3'
            env_path = '/usr/local/bin:/usr/bin:/bin'
            print("Virtual environment not found, using system Python")
            
        service_description = os.getenv('SERVICE_DESCRIPTION')
        if not service_description:
            service_description = "LeanTech Jobs Crowler"
        
        service_content = f"""[Unit]
Description={service_description}
After=network.target

[Service]
Type=simple
ExecStart={python_path} {app_path}
Restart=always
RestartSec=10
User=root
WorkingDirectory={script_dir}
Environment=PATH={env_path}:$PATH

[Install]
WantedBy=multi-user.target"""
        
        # Write to temporary file first, then move with sudo
        temp_file = f"/tmp/{service_name}.service"
        
        with open(temp_file, 'w') as f:
            f.write(service_content)
        
        # Move to systemd directory with sudo
        print(f"Copying service file to {service_file}")
        success, out, err = run_cmd(f"cp {temp_file} {service_file}", use_sudo=True)
        if not success:
            print(f"Error copying service file: {err}")
            return
        
        # Remove temporary file
        os.remove(temp_file)
        
        run_cmd("systemctl daemon-reload", use_sudo=True)
        run_cmd(f"systemctl enable {service_name}", use_sudo=True)
        print(f"Service {service_name} installed!")
    
    elif action == "start":
        success, out, err = run_cmd(f"systemctl start {service_name}", use_sudo=True)
        print("Service started!" if success else f"Error: {err}")
    
    elif action == "stop":
        success, out, err = run_cmd(f"systemctl stop {service_name}", use_sudo=True)
        print("Service stopped!" if success else f"Error: {err}")
    
    elif action == "uninstall":
        run_cmd(f"systemctl stop {service_name}", use_sudo=True)
        run_cmd(f"systemctl disable {service_name}", use_sudo=True)
        # Remove service file with sudo
        success, out, err = run_cmd(f"rm -f {service_file}", use_sudo=True)
        if not success:
            print(f"Error removing service file: {err}")
        run_cmd("systemctl daemon-reload", use_sudo=True)
        print("Service uninstalled!")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Usage: python3 service_manager.py <service> <action> [app_path]")
        sys.exit(1)
    
    service = sys.argv[1]
    action = sys.argv[2]
    app_path = sys.argv[3] if len(sys.argv) > 3 else None
    
    manage_service(service, action, app_path)