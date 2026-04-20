-- Willinks SQLite Database Schema

CREATE TABLE IF NOT EXISTS links (
    id TEXT PRIMARY KEY,
    slug TEXT UNIQUE NOT NULL,
    destination TEXT NOT NULL,
    created_at TEXT NOT NULL,
    expires_at TEXT,
    click_count INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_links_slug ON links(slug);