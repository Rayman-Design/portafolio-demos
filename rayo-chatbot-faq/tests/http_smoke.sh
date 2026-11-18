#!/usr/bin/env bash
# Prueba de punta a punta por HTTP, sin Google: levanta la demo y un servidor falso de Gemini.
# Uso: bash tests/http_smoke.sh
set -euo pipefail
cd "$(dirname "$0")/.."
WORK=$(mktemp -d)
cleanup() { kill ${P1:-} ${P2:-} 2>/dev/null || true; rm -rf "$WORK"; }
trap cleanup EXIT

echo "== 1) Modo simulado (config de ejemplo)"
php -S 127.0.0.1:8088 -t public >/dev/null 2>&1 & P1=$!
sleep 1
curl -s http://127.0.0.1:8088/api/chat.php; echo
curl -s -X POST -H 'Content-Type: application/json' -d '{"message":"¿Cómo es la forma de pago?"}' http://127.0.0.1:8088/api/chat.php; echo
kill $P1 2>/dev/null || true; sleep 0.5

echo "== 2) Modo en vivo contra servidor FALSO (comprueba cURL, encabezados y manejo de errores)"
mkdir -p "$WORK/app"
cp -r app/lib app/data "$WORK/app/"
echo "FAKE-KEY-solo-pruebas" > "$WORK/key.txt"
cat > "$WORK/app/config.php" <<PHP
<?php
\$c = require '$(pwd)/app/config.example.php';
\$c['mode'] = 'live';
\$c['free_tier_verified'] = true;
\$c['free_tier_verified_on'] = 'prueba local';
\$c['api_key_file'] = '$WORK/key.txt';
\$c['endpoint'] = 'http://127.0.0.1:8099/';
\$c['storage_dir'] = '$WORK/storage';
return \$c;
PHP
php -S 127.0.0.1:8099 tests/fake_gemini.php >/dev/null 2>&1 & P2=$!
RAYO_APP_DIR="$WORK/app" php -S 127.0.0.1:8088 -t public >/dev/null 2>&1 & P1=$!
sleep 1
curl -s http://127.0.0.1:8088/api/chat.php; echo
curl -s -X POST -H 'Content-Type: application/json' -d '{"message":"¿Hacen aplicaciones?"}' http://127.0.0.1:8088/api/chat.php; echo
curl -s -X POST -H 'Content-Type: application/json' -d '{"message":"prueba de cupo"}' -w ' [HTTP %{http_code}]' http://127.0.0.1:8088/api/chat.php; echo
echo "== 3) Los archivos públicos no contienen la llave"
if grep -R "FAKE-KEY" public >/dev/null; then echo "FALLA: llave en public/"; exit 1; else echo "OK"; fi
