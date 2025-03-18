#!/usr/bin/env python3
import os
import glob
import subprocess
import sys

def run_command(cmd, description):
    print(f"Running: {' '.join(cmd)}")
    try:
        subprocess.run(cmd, check=True)
    except subprocess.CalledProcessError as e:
        print(f"Error during {description}: {e}")
        sys.exit(1)

def process_conf_files(conf_dir, output_dir):
    """
    Process all .conf files in conf_dir, generating CRT, KEY, and PFX files in output_dir.
    Regenerate only if one or more of the files is missing.
    """
    conf_files = glob.glob(os.path.join(conf_dir, "*.conf"))
    
    if not conf_files:
        print("No .conf files found in", conf_dir)
        sys.exit(1)
    
    for conf_file in conf_files:
        # Use the file basename (without extension) as the domain name.
        domain = os.path.splitext(os.path.basename(conf_file))[0]
        print(f"\nProcessing {domain} using config file: {conf_file}")
        
        crt_file = os.path.join(output_dir, f"{domain}.crt")
        key_file = os.path.join(output_dir, f"{domain}.key")
        pfx_file = os.path.join(output_dir, f"{domain}.pfx")
        
        # Only regenerate if one or more files are missing.
        if not (os.path.exists(crt_file) and os.path.exists(key_file) and os.path.exists(pfx_file)):
            print(f"One or more files missing for {domain}; regenerating certificate set.")
            # Generate a self-signed certificate and key using OpenSSL with the .conf file.
            run_command([
                "openssl", "req", "-x509", "-nodes", "-days", "1825",
                "-newkey", "rsa:2048",
                "-keyout", key_file,
                "-out", crt_file,
                "-config", conf_file
            ], f"Generating certificate and key for {domain}")
            
            # Generate a PFX file from the certificate and key.
            run_command([
                "openssl", "pkcs12", "-export",
                "-out", pfx_file,
                "-inkey", key_file,
                "-in", crt_file,
                "-password", "pass:"  # using an empty password for now
            ], f"Generating PFX for {domain}")
            
            print(f"Finished processing {domain}:")
            print(f"  CRT: {crt_file}")
            print(f"  KEY: {key_file}")
            print(f"  PFX: {pfx_file}")
        else:
            print(f"All files exist for {domain}; skipping regeneration.")

def process_localhost_cert(output_dir):
    """
    Process the localhost certificate using dotnet dev-certs and OpenSSL.
    Regenerate only if one or more of localhost.crt, localhost.key, or localhost.pfx are missing.
    """
    # Define the file paths for localhost.
    crt_file = os.path.join(output_dir, "localhost.crt")
    key_file = os.path.join(output_dir, "localhost.key")
    pfx_file = os.path.join(output_dir, "localhost.pfx")
    
    # Only proceed if any file is missing.
    if not (os.path.exists(crt_file) and os.path.exists(key_file) and os.path.exists(pfx_file)):
        # Change working directory to output_dir so that files are placed there.
        os.chdir(output_dir)
        print("\nProcessing localhost certificate using dotnet dev-certs and OpenSSL...")
        
        # Run dotnet dev-certs to export the HTTPS certificate in PEM format.
        run_command([
            "dotnet", "dev-certs", "https", "--trust",
            "-ep", "localhost.crt",
            "-np",
            "--format", "PEM"
        ], "dotnet dev-certs export for localhost")
        
        # Create the PFX file for localhost using OpenSSL.
        run_command([
            "openssl", "pkcs12", "-export",
            "-out", "localhost.pfx",
            "-inkey", "localhost.key",
            "-in", "localhost.crt",
            "-passout", "pass:"
        ], "OpenSSL PFX creation for localhost")
        
        print("Finished processing localhost certificate:")
        print(f"  CRT: {crt_file}")
        print(f"  KEY: {key_file}")
        print(f"  PFX: {pfx_file}")
    else:
        print("\nAll localhost certificate files exist; skipping regeneration.")

def main():
    # Define the directories.
    # Output directory for generated certs: ../build-certs
    output_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "../certs"))
    # Directory containing the configuration files: ../certs/conf
    conf_dir = os.path.join(output_dir, "conf")
    
    # Process certificates for domains defined in .conf files.
    process_conf_files(conf_dir, output_dir)
    
    # Process localhost certificate with dotnet dev-certs.
    # This assumes that a localhost.key already exists in output_dir.
    process_localhost_cert(output_dir)

if __name__ == "__main__":
    main()
