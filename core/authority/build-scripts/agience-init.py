#!/usr/bin/env python3
import os
import sys
import subprocess
import time
import re
from pathlib import Path

# Get the directory of this script and project root
script_dir = Path(__file__).parent.absolute()
authority_dir = script_dir.parent
core_dir = authority_dir.parent
root_dir = core_dir.parent

def run_command(cmd, description, check=True):
    """Run a command and handle errors."""
    print(f"Running: {' '.join(cmd)}")
    try:
        result = subprocess.run(cmd, check=check, capture_output=True, text=True)
        if result.stdout:
            print(result.stdout)
        return result
    except subprocess.CalledProcessError as e:
        print(f"Error during {description}: {e}")
        if e.stdout:
            print(e.stdout)
        if e.stderr:
            print(e.stderr)
        if check:
            sys.exit(1)
        return e

def check_dependencies():
    """Check for required dependencies."""
    print("Checking dependencies...")
    dependencies = {
        "dotnet": "dotnet --version",
        "docker": "docker --version",
        "openssl": "openssl version",
        "npm": "npm -version"
    }
    
    missing = []
    for dep, cmd in dependencies.items():
        try:
            subprocess.run(cmd.split(), shell=True, check=True, capture_output=True)
            print(f"✓ {dep} is installed")
        except (subprocess.CalledProcessError, FileNotFoundError):
            missing.append(dep)
            print(f"✗ {dep} not found")
    
    if missing:
        print(f"\nMissing dependencies: {', '.join(missing)}")
        print("Please install them before continuing.")
        return False
    return True

def generate_certificates():
    """Generate certificates"""
    print("\nGenerating certificates...")
    cert_script = script_dir / "generate-certs.py"
    run_command([sys.executable, str(cert_script)], "certificate generation")

def generate_strong_password(length=16):
    """Generate a secure random password."""
    import secrets
    import string
    alphabet = string.ascii_letters + string.digits + "!@#$%^&*()-_=+"
    password = ''.join(secrets.choice(alphabet) for i in range(length))
    return password

def prompt_for_value(prompt_text, default=None):
    """Prompt the user for a value, with optional default and secrecy."""
    default_text = f" [{default}]" if default else ""
    prompt = f"{prompt_text}{default_text}: "
    
    while True:
        value = input(prompt)
        
        # Use default if nothing entered
        if not value and default:
            return default   
            
        return value
    
def replace_env_value(env_content, key, new_value):
    pattern = rf"({key}=).*"
    return re.sub(pattern, rf"\1{new_value}", env_content)

def create_env_file():
    """Create a .env file with development settings if it doesn't exist."""
    env_file = authority_dir / ".env"
    env_example_path = authority_dir / ".env.example"
       
    if env_file.exists() and env_example_path.exists():
        should_overwrite = prompt_for_value("\nEnvironment file already exists. Overwrite? (y/n)", default='n').lower()
        if should_overwrite != 'y':
            print("Keeping existing .env file.")
            return

    if not env_example_path.exists():
        print("Could not find .env.example")
        return

    # Read the .env.example file    
    with open(env_example_path, 'r') as f:
        env_content = f.read()

    print("\nCreating development .env file...")
    
    # Replace placeholders in the template    
    env_content = replace_env_value(env_content, 'DATABASE_PASSWORD', generate_strong_password())

    # Ask for Google OAuth credentials
    google_client_id = prompt_for_value("Google OAuth Client ID")
    if google_client_id:
        env_content = replace_env_value(env_content, 'GOOGLE_OAUTH_CLIENT_ID', google_client_id)

    google_client_secret = prompt_for_value("Google OAuth Client Secret")
    if google_client_secret:
        env_content = replace_env_value(env_content, 'GOOGLE_OAUTH_CLIENT_SECRET', google_client_secret)
    
    # Write the updated content to .env file
    with open(env_file, 'w') as f:
        f.write(env_content)
        
    print(f"Created {env_file} with generated database password and your settings.")

def update_env_file(key, value):
    """Update a key-value pair in the .env file."""
    env_file = authority_dir / ".env"

    if env_file.exists():
        with open(env_file, "r") as file:
            lines = file.readlines()

        with open(env_file, "w") as file:
            key_found = False
            for line in lines:
                if line.startswith(f"{key}="):
                    file.write(f"{key}={value}\n")
                    key_found = True
                else:
                    file.write(line)

            if not key_found:
                file.write(f"{key}={value}\n")

    else:
        with open(env_file, "w") as file:
            file.write(f"{key}={value}\n")

def initialize_database():
    """Initialize the database using Entity Framework migrations."""
    print("\nInitializing database...")

    # Ensure LAN_EXTERNAL_AUTHORITY is set to TRUE in .env
    update_env_file("LAN_EXTERNAL_AUTHORITY", "true")

    # First, start the database container
    os.chdir(authority_dir)
    run_command(["docker-compose", "up", "--build", "-d", "database-sql"], "starting database service")

    # Wait for the database to be ready
    print("Waiting for database to be ready...")
    time.sleep(5)  # Simple wait - in a more robust script, we'd check service health

    # Run Entity Framework migrations
    identity_api_dir = authority_dir / "identity-api-dotnet"
    os.chdir(identity_api_dir)

    # Load environment variables
    
    env = os.environ.copy()
    env["LAN_EXTERNAL_AUTHORITY"] = "true"
    env["EF_MIGRATION"] = "TRUE"
    env["ENV_FILE_PATH"] = str(authority_dir / ".env") #TODO: load .env file instead of passing ENV_FILE_PATH

    try:
        print("Running database migrations...")
        result = subprocess.run(
            ["dotnet", "ef", "database", "update"],
            check=True,
            capture_output=True,
            text=True,
            env=env
        )
        if result.stdout:
            print(result.stdout)
        print("Database initialization completed successfully.")
    except subprocess.CalledProcessError as e:
        print(f"Error during database migration: {e}")
        if e.stdout:
            print(e.stdout)
        if e.stderr:
            print(e.stderr)
        print("Database initialization failed.")

def main():
    print("=== Agience Development Environment Initialization ===")
    
    if not check_dependencies():
        return
    
    create_env_file()
    generate_certificates()
    
    # Ask if user wants to initialize the database
    should_init_db = prompt_for_value("\nInitialize database? (y/n)", default='y').lower()
    if should_init_db == 'y':
        initialize_database()    
    
    print("\n=== Development Environment Setup Complete ===")

if __name__ == "__main__":
    main()