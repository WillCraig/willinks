#!/bin/bash
set -e

REPO_DIR=/opt/willinks-src
PUBLISH_DIR=/opt/willinks

echo ">>> Pulling latest..."
cd "$REPO_DIR"
git pull origin main

echo ">>> Building..."
dotnet publish src/Willinks.Api/Willinks.Api.csproj -c Release -o "$PUBLISH_DIR"

echo ">>> Restarting service..."
sudo systemctl restart willinks

echo ">>> Done."
