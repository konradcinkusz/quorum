#!/bin/bash
# Runs on the local Postgres container's first boot only (postgres entrypoint convention).
# One dev user owns both databases; the deployed estate separates the roles.
set -e
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
  CREATE DATABASE authdb;
EOSQL
