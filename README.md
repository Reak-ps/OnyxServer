# 🖤 OnyxServer

A lightweight static file HTTP server built with C# and .NET — no dependencies, no bloat. Just drop in a config file and serve.

---

## Features

- Serves static files with correct MIME types (HTML, CSS, JS, PNG, JPG, and more)
- Directory listing — browse folder contents directly in the browser with a custom template
- Config-driven setup via a simple `.conf` file
- Persistent logging to `server.log` with timestamps and readable status labels
- Async request handling with `HttpListener`
- Automatic fallback to a default file when hitting `/`
- Directory traversal protection — blocks `..` requests with a 403 response
- Custom 404 and 403 error pages served from config

---

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or later recommended)

### Clone & Run

```bash
git clone https://github.com/your-username/OnyxServer.git
cd OnyxServer
dotnet run
```

---

## Configuration

OnyxServer reads its config from `ONYXSERVER.conf` in the **project root**.

```
port=8080
ip=localhost
folder=public
file=index.html
notfound=404.html
forbidden=403.html
dirtemplate=dir.html
```

| Key           | Description                                          | Example         |
|---------------|------------------------------------------------------|-----------------|
| `port`        | Port to listen on                                    | `8080`          |
| `ip`          | IP address or hostname to bind to                    | `localhost`     |
| `folder`      | Folder to serve files from                           | `public`        |
| `file`        | Default file served when hitting `/`                 | `index.html`    |
| `notfound`    | Custom 404 error page (inside `folder`)              | `404.html`      |
| `forbidden`   | Custom 403 error page (inside `folder`)              | `403.html`      |
| `dirtemplate` | HTML template for directory listings                 | `dir.html`      |

The directory listing template uses `###FILE_LIST###` as a placeholder — OnyxServer replaces it with the actual file links at runtime.

---

## Project Structure

```
OnyxServer/
├── ONYXSERVER.conf        ← config file
├── server.log             ← auto-generated request log
├── public/                ← your static files go here
│   ├── index.html
│   ├── 404.html
│   ├── 403.html
│   └── dir.html
└── OnyxServer/
    └── Program.cs
```

---

## Logging

Every request and error is logged to `server.log` with a timestamp and status label:

```
[15.06.2026 21:04:12] [200] OK DELIVERED: /index.html
[15.06.2026 21:04:13] [200] OK DELIVERED: /style.css
[15.06.2026 21:04:14] [DIR] LISTED: /assets
[15.06.2026 21:04:15] [403] NO TOUCHY FORBIDDEN: /../../../etc/passwd
[15.06.2026 21:04:16] [404] Not Found NOT FOUND: /public/favicon.ico
```

---

## MIME Type Support

| Extension      | MIME Type                        |
|----------------|----------------------------------|
| `.html`        | `text/html; charset=utf-8`       |
| `.css`         | `text/css`                       |
| `.js`          | `application/javascript`         |
| `.png`         | `image/png`                      |
| `.jpg / .jpeg` | `image/jpeg`                     |
| other          | `application/octet-stream`       |

---

## Roadmap

### 🔧 Core Improvements
- [ ] **Modular folder contents** — cleaner, more flexible directory listing system
- [ ] **Error pages in separate folder** — move 404/403 pages out of the public directory
- [ ] **Dynamic MIME types** — extend and improve MIME type detection
- [ ] **Professional logging** — structured, leveled log output
- [ ] **Query parameter parsing** — read and handle URL query strings
- [ ] **System files vs. public files** — separate internal server files from served content

### 🌐 API
- [ ] **API endpoints & JSON** — serve JSON responses from defined endpoints

### 🔒 Security & Platform
- [ ] **SSL / HTTPS** — serve over TLS with certificate configuration
- [ ] **Linux support** — cross-platform compatibility

---

## Built With

- C# / .NET
- `System.Net.HttpListener` — built-in .NET HTTP server
- No external dependencies

---

## License

MIT — do whatever you want with it.
