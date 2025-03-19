#!/usr/bin/env python3
import os
import sys
import subprocess
import time
import re
from pathlib import Path

# Get the directory of this script and project root
script_dir = Path(__file__).parent.absolute()
build_dir = script_dir.parent
authority_dir = build_dir.parent
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
        "docker": "docker info",
        "openssl": "openssl version",
        "npm": "npm --version"  # corrected command flag
    }
    
    missing = []
    for dep, cmd in dependencies.items():
        try:
            # Pass the command as a string (without splitting) if shell=True.
            result = subprocess.run(
                cmd,
                shell=True,
                check=True,
                capture_output=True,
                text=True
            )
            # Try to capture version info from stdout or stderr.
            output = result.stdout.strip() or result.stderr.strip()
            print(f"✓ {dep} is available: {output}")
        except (subprocess.CalledProcessError, FileNotFoundError) as e:
            missing.append(dep)
            print(f"✗ {dep} not found (error: {e})")
    
    if missing:
        print("Missing dependencies:", ", ".join(missing))
        return False
    else:
        print("All dependencies are present.")
        return True

def generate_certificates():
    """Generate certificates"""
    print("\nGenerating certificates...")
    cert_script = script_dir / "generate_certs.py"
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
    pattern = rf"^(?P<key>{re.escape(key)}=).*"
    return re.sub(pattern, rf"\g<key>{new_value}", env_content, flags=re.MULTILINE)

def get_env_value(key):
    """Get a value from the .env file for the given key."""
    env_file = authority_dir / ".env"
    if env_file.exists():
        with open(env_file, "r", encoding='utf-8-sig') as file:
            for line in file:
                if line.startswith(f"{key}="):
                    # Remove key and split on '='
                    parts = line.strip().split("=", 1)
                    if len(parts) == 2:
                        return parts[1]
    return None

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
    with open(env_example_path, 'r', encoding='utf-8-sig') as f:
        env_content = f.read()

    print("\nCreating development .env file...")
    
    # Replace placeholders in the template    
    env_content = replace_env_value(env_content, 'DATABASE_PASSWORD', generate_strong_password())

    # Ask for Google OAuth credentials
    google_client_id = prompt_for_value("Enter your Google OAuth Client ID")
    if google_client_id:
        env_content = replace_env_value(env_content, 'GOOGLE_OAUTH_CLIENT_ID', google_client_id)

    google_client_secret = prompt_for_value("Enter your Google OAuth Client Secret")
    if google_client_secret:
        env_content = replace_env_value(env_content, 'GOOGLE_OAUTH_CLIENT_SECRET', google_client_secret)
    
    # Write the updated content to .env file
    with open(env_file, 'w',  encoding='utf-8-sig') as f:
        f.write(env_content)
        
    print(f"Created {env_file} with generated database password and your settings.")

def update_env_file(key, value):
    """Update a key-value pair in the .env file."""
    env_file = authority_dir / ".env"

    if env_file.exists():
        with open(env_file, "r", encoding='utf-8-sig') as file:
            lines = file.readlines()

        with open(env_file, "w", encoding='utf-8-sig') as file:
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
        with open(env_file, "w", encoding='utf-8-sig') as file:
            file.write(f"{key}={value}\n")

def initialize_database():
    """Initialize the database using Entity Framework migrations."""
    print("\nInitializing database...")

    # Save the original value of LAN_EXTERNAL_AUTHORITY from the .env file
    lan_external_authority_original = get_env_value("LAN_EXTERNAL_AUTHORITY")
    
    # Ensure LAN_EXTERNAL_AUTHORITY is set to true in .env for database initialization
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
    env["EF_MIGRATION"] = "TRUE"
    env["LAN_EXTERNAL_AUTHORITY"] = "true"
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
    finally:
        # Restore the original value of LAN_EXTERNAL_AUTHORITY from the .env file
        if lan_external_authority_original is not None:
            update_env_file("LAN_EXTERNAL_AUTHORITY", lan_external_authority_original)
            print("Restored LAN_EXTERNAL_AUTHORITY to its original value.")
        else:
            print("No original value for LAN_EXTERNAL_AUTHORITY was found to restore.")

def run_npm_install():
    """Run npm install in the manage-ui directory."""
    # Adjust the path to your manage-ui directory if needed.
    manage_ui_dir = authority_dir / "manage-ui"
    print(f"\nRunning npm install in {manage_ui_dir} ...")
    os.chdir(manage_ui_dir)
    # On Windows, use "npm.cmd" instead of "npm"
    npm_cmd = "npm.cmd" if sys.platform == "win32" else "npm"
    run_command([npm_cmd, "install"], "npm install in manage-ui directory")

def initialize():
    print("=== Agience Development Environment Initialization ===")
    
    if not check_dependencies():
        return
    
    create_env_file()
    generate_certificates()
    run_npm_install() 
    
    # Ask if user wants to initialize the database
    should_init_db = prompt_for_value("\nInitialize database? (y/n)", default='y').lower()
    if should_init_db == 'y':
        initialize_database()    
    
    print("\n=== Development Environment Setup Complete ===")

if __name__ == "__main__":
    initialize()
