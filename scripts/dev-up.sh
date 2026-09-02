#!/usr/bin/env bash
#
# One command for the local backing services: generates the dev signing key on first run
# (RS256 locally too — see docker-compose.yml for why), then brings up Postgres and the
# authservice instance. Windows: run scripts/generate-jwt-signing-key.ps1 -Path
# .dev/keys/authservice-dev.pem once, then `docker compose up -d`.

set -euo pipefail

cd "$(dirname "$0")/.."

# The secret-scanning pre-commit hook, activated here because this is the script every
# contributor already runs first. CI catches a committed credential after the commit exists;
# the hook is what stops it being written. See .githooks/pre-commit and issue #20.
if [ "$(git config --get core.hooksPath || true)" != ".githooks" ]; then
  git config core.hooksPath .githooks
  echo "Enabled the pre-commit secret scan (git config core.hooksPath .githooks)."
fi

KEY=".dev/keys/authservice-dev.pem"
if [ ! -f "$KEY" ]; then
  mkdir -p .dev/keys
  scripts/generate-jwt-signing-key.sh "$KEY"
  echo "Generated the LOCAL development signing key at $KEY (git-ignored, never deployed)."
fi

docker compose up -d
echo
echo "Postgres:      localhost:5432 (quorum/quorum; quorumdb + authdb)"
echo "authservice:   http://localhost:8080 (admin@quorum.local / Admin123!)"
echo "Now run:       dotnet run --project Server"
