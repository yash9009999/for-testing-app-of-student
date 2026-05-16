#!/usr/bin/env bash
set -euo pipefail

API="${API_URL:-http://localhost:5084/api}"
AUTH="${AUTH_URL:-http://localhost:5001/api/auth}"

BASKET='{"promotion":null,"basketItems":[{"products":[{"productId":1},{"productId":4}]}]}'

echo "== Create guest order =="
CREATE=$(curl -sf -X POST "$API/order" -H 'Content-Type: application/json' -d "$BASKET")
ORDER_ID=$(echo "$CREATE" | python3 -c "import sys,json; print(json.load(sys.stdin)['orderId'])")
GUEST_TOKEN=$(echo "$CREATE" | python3 -c "import sys,json; print(json.load(sys.stdin).get('guestAccessToken') or '')")
echo "orderId=$ORDER_ID guestTokenPresent=$([ -n "$GUEST_TOKEN" ] && echo yes || echo no)"

echo "== IDOR: fetch without guest token (expect 404) =="
CODE=$(curl -s -o /dev/null -w '%{http_code}' "$API/order/$ORDER_ID")
test "$CODE" = "404" && echo "PASS" || { echo "FAIL http=$CODE"; exit 1; }

echo "== Authorized fetch with guest token (expect 200) =="
CODE=$(curl -s -o /dev/null -w '%{http_code}' -H "X-Guest-Token: $GUEST_TOKEN" "$API/order/$ORDER_ID")
test "$CODE" = "200" && echo "PASS" || { echo "FAIL http=$CODE"; exit 1; }

echo "== Login =="
LOGIN=$(curl -sf -X POST "$AUTH/login" -H 'Content-Type: application/json' -d '{"username":"testuser","password":"password123"}')
JWT=$(echo "$LOGIN" | python3 -c "import sys,json; print(json.load(sys.stdin)['token'])")

echo "== Create owned order with bearer =="
OWNED=$(curl -sf -X POST "$API/order" -H 'Content-Type: application/json' -H "Authorization: Bearer $JWT" -d "$BASKET")
OWNED_ID=$(echo "$OWNED" | python3 -c "import sys,json; print(json.load(sys.stdin)['orderId'])")

echo "== Owned order readable with bearer (expect 200) =="
CODE=$(curl -s -o /dev/null -w '%{http_code}' -H "Authorization: Bearer $JWT" "$API/order/$OWNED_ID")
test "$CODE" = "200" && echo "PASS" || { echo "FAIL http=$CODE"; exit 1; }

echo "== Guest order not readable with wrong bearer only (expect 404) =="
CODE=$(curl -s -o /dev/null -w '%{http_code}' -H "Authorization: Bearer $JWT" "$API/order/$ORDER_ID")
test "$CODE" = "404" && echo "PASS" || { echo "FAIL http=$CODE"; exit 1; }

echo "All security smoke tests passed."
