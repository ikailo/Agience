#!/usr/bin/env python3
import sys
from core.authority.build.scripts import agience_init

def main():
    try:
        agience_init.initialize()
    except Exception as e:
        print(f"Initialization failed: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
