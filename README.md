# Willinks

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)

> A self-hosted personal URL shortener with click tracking and expiration.

---

## Features

- **Auto-generated slugs** — random short codes out of the box
- **Custom slugs** — bring your own memorable alias
- **Click tracking** — per-link click counts
- **Expiration** — set an optional expiry date per link
- **API key auth** — single shared key protects all write operations
- **Simple deployment** — runs as a systemd service on any Linux VPS

---

## Screenshot

// TODO: add screenshot here

---

## Deployment (DigitalOcean Droplet)

Deploys automatically via GitHub Actions on push to `main` — the workflow SSHs into the droplet and runs a deploy script.

Set the following GitHub Actions secrets:

| Secret           | Description                        |
|------------------|------------------------------------|
| `DROPLET_HOST`   | IP or hostname of your droplet     |
| `DROPLET_USER`   | SSH username (e.g. `root`)         |
| `SSH_PRIVATE_KEY`| Private key for SSH access         |

On the droplet, environment variables (`DATABASE_URL`, `API_KEY`) should be set in the systemd service unit or a sourced env file.

Production routing is split intentionally:

- `links.willc.pro` proxies directly to the Willinks app for the admin UI
- `willc.pro` serves the primary site from `/var/www/jekyll` first
- requests on `willc.pro` that do not match a real static page fall through to Willinks so `/{slug}` shortlinks still work

---

## Local Development

```bash
# Create local database
sqlite3 willinks.db < deploy/willinks.db.sql

# Run the app
dotnet run --project src/Willinks.Api
```

In development, the app falls back to:

- `DATABASE_URL=Data Source=willinks.db`
- `API_KEY=dev-secret`

That means no extra env export is required for the normal local workflow. The app will be available at `http://localhost:5000`.

### Testing

```bash
dotnet test
```

The integration tests create their own temporary SQLite database, apply `deploy/willinks.db.sql`, and inject test configuration automatically. Your local `willinks.db` and `.env` are not required for the test suite.

---

## Configuration

| Variable       | Description                                   |
| -------------- | --------------------------------------------- |
| `DATABASE_URL` | SQLite connection string (e.g., `Data Source=willinks.db`) |
| `API_KEY`      | Shared secret for authenticating API requests |

---

## API Reference

All write endpoints require the `X-Api-Key` header.

| Method   | Path                | Auth | Description                 |
| -------- | ------------------- | ---- | --------------------------- |
| `GET`    | `/{slug}`           | —    | Redirect to destination URL |
| `GET`    | `/api/links`        | Yes  | List all links              |
| `POST`   | `/api/links`        | Yes  | Create a new short link     |
| `DELETE` | `/api/links/{slug}` | Yes  | Delete a link by slug       |

### POST `/api/links` body

```json
{
  "destination": "https://example.com/very/long/url",
  "slug": "my-link",
  "expiresAt": "2025-12-31T00:00:00Z"
}
```

`slug` and `expiresAt` are optional.

---

## Tech Stack

- **Runtime**: .NET 9
- **Database**: SQLite (self-contained, file-based)
- **ORM**: Dapper
- **Frontend**: Vanilla JS (no build step)
