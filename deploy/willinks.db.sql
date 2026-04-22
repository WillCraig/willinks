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

INSERT OR IGNORE INTO links (id, slug, destination, created_at, expires_at, click_count)
VALUES (
    'seed-gh',
    'gh',
    'https://github.com/WillCraig',
    '2026-04-22T00:00:00Z',
    NULL,
    0
);
