#!/usr/bin/env bash
# Start Scoops2Go dev stack (auth-server, API, Vite). Requires .NET SDK and Node.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export DOTNET_ROOT="${DOTNET_ROOT:-/usr/local/opt/dotnet/libexec}"
export PATH="$DOTNET_ROOT:$PATH"

echo "Starting auth-server on http://localhost:5001 ..."
(cd "$ROOT/auth-server" && dotnet run --urls http://localhost:5001) &
AUTH_PID=$!

echo "Starting API on http://localhost:5084 ..."
(cd "$ROOT/api" && dotnet run --urls http://localhost:5084) &
API_PID=$!

sleep 4

echo "Starting Vite on http://localhost:5173 ..."
(cd "$ROOT/app" && node node_modules/vite/bin/vite.js --host) &
VITE_PID=$!

trap 'kill $AUTH_PID $API_PID $VITE_PID 2>/dev/null; exit' INT TERM

echo ""
echo "Scoops2Go is running:"
echo "  App:  http://localhost:5173"
echo "  API:  http://localhost:5084"
echo "  Auth: http://localhost:5001"
echo "  Login: testuser / password123"
echo "Press Ctrl+C to stop all services."
wait
