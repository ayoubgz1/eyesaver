#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd "$DIR"

# Find best python3
if command -v python3 >/dev/null 2>&1; then
    PYTHON_CMD="python3"
elif [ -f "/usr/bin/python3" ]; then
    PYTHON_CMD="/usr/bin/python3"
else
    PYTHON_CMD="python"
fi

echo "=========================================="
echo " Starting EyeSaver (20-20-20 Rule)        "
echo "=========================================="
echo "Work interval: 20 minutes"
echo "Break duration: 20 seconds"
echo ""

$PYTHON_CMD eyesaver.py
