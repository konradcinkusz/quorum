#!/usr/bin/env bash
#
# Generates a 2048-bit RSA private key in PKCS#8 PEM ("BEGIN PRIVATE KEY") — the format
# authservice's Jwt__PrivateKeyPem expects. Windows contributors: use
# scripts/generate-jwt-signing-key.ps1 instead; openssl is not a command there.
#
#   scripts/generate-jwt-signing-key.sh                # prints to stdout
#   scripts/generate-jwt-signing-key.sh path/to/key.pem
#
# Generated, never invented: a human asked to make up a secret produces "changeme".
# A key generated for local development is a development convenience, not a trust root —
# never promote one to a deployed environment.

set -euo pipefail

OUT="${1:-}"

if ! command -v openssl >/dev/null 2>&1; then
  echo "openssl was not found. On Windows use scripts/generate-jwt-signing-key.ps1;"
  echo "elsewhere install openssl and re-run."
  exit 1
fi

if [ -z "$OUT" ]; then
  openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048
else
  umask 077
  openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out "$OUT"
  echo "Wrote $OUT"
fi
