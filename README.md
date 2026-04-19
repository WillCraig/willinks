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

---

## Configuration

| Variable       | Description                                                                               |
| -------------- | ----------------------------------------------------------------------------------------- |
| `DATABASE_URL` | SQLite connection string (e.g. `"Data Source=/var/lib/willinks/willinks.db;Cache=Shared"` — quote when placing in a systemd env file, since the value contains a space) |
| `API_KEY`      | Shared secret for authenticating API requests                                             |

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
- **Database**: SQLite (file-based, auto-provisioned on first run)
- **ORM**: Dapper
- **Frontend**: Vanilla JS (no build step)
