#!/usr/bin/env bash
set -euo pipefail
: "${POSTGRES_URL:=postgres://payment_user:payment_pass@localhost:5433/payment?sslmode=disable}"
echo "Applying migrations to $POSTGRES_URL"
psql "$POSTGRES_URL" -f ./internal/adapters/postgres/migrations/001_init.sql
echo "Done"
