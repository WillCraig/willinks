#!/bin/bash
set -e

REPO_DIR=/opt/willinks-src
PUBLISH_DIR=/opt/willinks
DB_FILE=/opt/willinks/willinks.db

echo ">>> Pulling latest..."
cd "$REPO_DIR"
git pull origin main

echo ">>> Initializing database..."
if [ ! -f "$DB_FILE" ]; then
    sqlite3 "$DB_FILE" < "$REPO_DIR/deploy/willinks.db.sql"
    echo ">>> Database created at $DB_FILE"
else
    echo ">>> Database already exists, applying schema and seed"
    sqlite3 "$DB_FILE" < "$REPO_DIR/deploy/willinks.db.sql"
fi

echo ">>> Building..."
dotnet publish src/Willinks.Api/Willinks.Api.csproj -c Release -o "$PUBLISH_DIR"

echo ">>> Restarting service..."
sudo systemctl restart willinks

echo ">>> Done."
