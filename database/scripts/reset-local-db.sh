#!/usr/bin/env bash
# Resetuje lokální vývojovou databázi VPSI
set -euo pipefail

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-vpsi}"
DB_USER="${DB_USER:-vpsi}"

echo "Resetuji databázi ${DB_NAME} na ${DB_HOST}:${DB_PORT}..."

PGPASSWORD="${DB_PASSWORD:-vpsi_dev}" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS ${DB_NAME};"
PGPASSWORD="${DB_PASSWORD:-vpsi_dev}" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "CREATE DATABASE ${DB_NAME};"

# Aplikuj migrace
for migration in "$(dirname "$0")/../migrations"/V*.sql; do
  if [ -f "$migration" ]; then
    echo "Aplikuji migraci: $(basename "$migration")"
    PGPASSWORD="${DB_PASSWORD:-vpsi_dev}" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$migration"
  fi
done

# Aplikuj seed data
for seed in "$(dirname "$0")/../seed"/*.sql; do
  if [ -f "$seed" ]; then
    echo "Nahrávám seed: $(basename "$seed")"
    PGPASSWORD="${DB_PASSWORD:-vpsi_dev}" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$seed"
  fi
done

echo "Databáze ${DB_NAME} úspěšně resetována."
