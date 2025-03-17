#!/usr/bin/env python3

import os
import subprocess
import argparse
import sys
import time

# Define environment configurations
ENVIRONMENTS = {
    "identity_internal": {
        "description": "Identity running internally (in Docker)",
        "lan_external_authority": "false"        
    },
    "identity_external": {
        "description": "Identity API and Manage UI running externally (outside Docker)",
        "lan_external_authority": "true",
        "cert_config": "WAN certificates"
    }
}

def run_command(cmd, description=None):
    """Run a shell command and print its output."""
    if description:
        print(f"\n{description}:")
    print(f"Running: {' '.join(cmd)}")
    try:
        result = subprocess.run(cmd, check=True, capture_output=True, text=True)
        if result.stdout:
            print(result.stdout)
        return True
    except subprocess.CalledProcessError as e:
        print(f"Error: {e}")
        if e.stdout:
            print(e.stdout)
        if e.stderr:
            print(e.stderr)
        return False

def update_env_file(env_name):
    """Update or create .env file with environment configuration."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    authority_dir = os.path.dirname(script_dir)
    env_file_path = os.path.join(authority_dir, ".env")
    
    if env_name not in ENVIRONMENTS:
        print(f"Error: Unknown environment '{env_name}'")
        return False
    
    env_config = ENVIRONMENTS[env_name]
    
    # Read existing .env file if it exists
    env_vars = {}
    if os.path.exists(env_file_path):
        with open(env_file_path, 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    key, value = line.split('=', 1)
                    env_vars[key] = value
    
    # Update environment-specific values
    env_vars['LAN_EXTERNAL_AUTHORITY'] = env_config['lan_external_authority']
        
    # Write updated .env file
    with open(env_file_path, 'w') as f:
        for key, value in sorted(env_vars.items()):
            f.write(f"{key}={value}\n")
    
    print(f"Updated .env file at {env_file_path}")
    print(f"LAN_EXTERNAL_AUTHORITY set to {env_config['lan_external_authority']}")
    return True

def restart_docker_containers():
    """Restart Docker containers using docker-compose."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    authority_dir = os.path.dirname(script_dir)
    
    print("Stopping Docker containers...")
    if not run_command(["docker-compose", "-f", os.path.join(authority_dir, "docker-compose.yml"), "down"], 
                     "Stopping containers"):
        return False
    
    # Give Docker a moment to fully release resources
    time.sleep(2)
    
    print("Starting Docker containers...")
    return run_command(["docker-compose", "-f", os.path.join(authority_dir, "docker-compose.yml"), "up", "-d"], 
                     "Starting containers")

def switch_environment(env_name):
    """Switch to the specified environment configuration."""
    if env_name not in ENVIRONMENTS:
        print(f"Error: Unknown environment '{env_name}'")
        return False
    
    print(f"\nSwitching to {env_name} environment:")
    print(f"  {ENVIRONMENTS[env_name]['description']}")   
    
    # Update .env file
    if not update_env_file(env_name):
        return False
    
    # Restart Docker containers
    if not restart_docker_containers():
        return False
    
    print(f"\nSuccessfully switched to {env_name} environment")
    return True

def list_environments():
    """List available environment configurations."""
    print("\nAvailable environment configurations:")
    for name, config in ENVIRONMENTS.items():
        print(f"  - {name}: {config['description']}")

def main():
    parser = argparse.ArgumentParser(description="Manage Agience development environments")
    subparsers = parser.add_subparsers(dest="command", help="Command to execute")
    
    # Switch environment command
    switch_parser = subparsers.add_parser("switch", help="Switch to a specific environment")
    switch_parser.add_argument("environment", choices=ENVIRONMENTS.keys(), help="Environment to switch to")
    
    # List environments command
    list_parser = subparsers.add_parser("list", help="List available environments")
    
    # Restart containers command
    restart_parser = subparsers.add_parser("restart", help="Restart Docker containers")
    
    args = parser.parse_args()
    
    if not args.command or args.command == "list":
        list_environments()
    elif args.command == "switch":
        if not switch_environment(args.environment):
            sys.exit(1)
    elif args.command == "restart":
        if not restart_docker_containers():
            sys.exit(1)
    else:
        parser.print_help()

if __name__ == "__main__":
    main()