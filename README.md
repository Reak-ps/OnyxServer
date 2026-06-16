# 🖤 OnyxServer

A lightweight static file HTTP server built with C# and .NET — no dependencies, no bloat. Just drop in a config file and serve.

---

## Features

- Serves static files with correct MIME types (HTML, CSS, JS, PNG, JPG, and more)
- Directory listing — browse folder contents directly in the browser
- Config-driven setup via a simple `.conf` file
- Persistent logging to `server.log` with timestamps
- Async request handling with `HttpListener`
- Automatic fallback to a default file when hitting `/`
- Clean console output for every request and error

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
```

| Key      | Description                                      | Example         |
|----------|--------------------------------------------------|-----------------|
| `port`   | Port to listen on                                | `8080`          |
| `ip`     | IP address or hostname to bind to                | `localhost`     |
| `folder` | Folder to serve files from                       | `public`        |
| `file`   | Default file served when hitting `/`             | `index.html`    |

---

## Project Structure

```
OnyxServer/
├── ONYXSERVER.conf        ← config file
├── server.log             ← auto-generated request log
├── public/                ← your static files go here
│   └── index.html
└── OnyxServer/
    └── Program.cs
```

---

## Logging

Every request and error is logged to `server.log` in the project root with a timestamp:

```
[15.06.2026 21:04:12] [OKAY] DELIVERED: /index.html
[15.06.2026 21:04:13] [OKAY] DELIVERED: /style.css
[15.06.2026 21:04:14] [DIR] LISTED: /assets
[15.06.2026 21:04:15] [ERROR 404] NOT FOUND: /../../../public/favicon.ico
```

---

## MIME Type Support

OnyxServer automatically sets the correct `Content-Type` header based on file extension:

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

OnyxServer is actively being developed. Here's what's planned:

### 🔧 Core Improvements
- [ ] **CLI Arguments** — pass port, folder, and other options directly via command line instead of relying solely on the `.conf` file
- [ ] **Multi-threading / performance** — handle concurrent requests without blocking using proper async pipelines

### 🔒 Security
- [ ] **HTTPS / SSL support** — serve over TLS with certificate configuration
- [ ] **Basic Auth** — protect routes or the entire server with username/password authentication

### 🌐 Hosting
- [ ] **Virtual Hosts** — serve multiple sites from one server instance, routing by domain or subdomain

---

## Built With

- C# / .NET
- `System.Net.HttpListener` — built-in .NET HTTP server
- No external dependencies

---

## License

MIT — do whatever you want with it.