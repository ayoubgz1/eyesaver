#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd "$DIR"

if command -v python3 >/dev/null 2>&1; then
    PYTHON_CMD="python3"
elif [ -f "/usr/bin/python3" ]; then
    PYTHON_CMD="/usr/bin/python3"
else
    PYTHON_CMD="python"
fi

echo "=========================================="
echo " Starting EyeSaver in Quick TEST Mode     "
echo "=========================================="
echo "Test interval: 5 seconds work -> 5 seconds break"
echo ""

$PYTHON_CMD eyesaver.py --test
