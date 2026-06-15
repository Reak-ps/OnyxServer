# 🖤 OnyxServer

A lightweight HTTP server built with C# and .NET — serves static files from a local folder over the network with zero dependencies.

---

## Features

- Serves static files (HTML, CSS, JS, etc.) from a configurable directory
- Config-driven setup via a simple `.conf` file — no hardcoded values
- Async request handling with `HttpListener`
- Automatic fallback to a default file when hitting `/`
- Clean console logging for delivered files and errors

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

OnyxServer reads its config from a file called `ONYXSERVER.conf` located in the **project root** (three levels above the binary output).

```
port=8080
ip=localhost
folder=public
file=index.html
```

| Key      | Description                              | Example         |
|----------|------------------------------------------|-----------------|
| `port`   | Port to listen on                        | `8080`          |
| `ip`     | IP address or hostname to bind to        | `localhost`     |
| `folder` | Folder to serve files from               | `public`        |
| `file`   | Default file served when hitting `/`     | `index.html`    |

> Make sure the `folder` you specify actually exists in the project root and contains your files.

---

## Project Structure

```
OnyxServer/
├── ONYXSERVER.conf       ← config file
├── public/               ← your static files go here
│   └── index.html
└── OnyxServer/
    └── Program.cs
```

---

## Example Output

```
Listening on port 8080
[OKAY] DELIVERED: /index.html
[OKAY] DELIVERED: /style.css
[INTERGALCTIC ERROR] FILE NOT FOUND: /../../../public/favicon.ico
```

---

## Roadmap

OnyxServer is just getting started. Here's what's planned:

### 🔧 Core Improvements
- [ ] **MIME Type support** — correctly set `Content-Type` headers for HTML, CSS, JS, images, fonts, and more so browsers handle files properly
- [ ] **CLI Arguments** — pass port, folder, and other options directly via command line instead of relying solely on the `.conf` file
- [ ] **Multi-threading / performance** — handle concurrent requests properly without blocking, using async pipelines and connection pooling

### 🔒 Security
- [ ] **HTTPS / SSL support** — serve over TLS with certificate configuration
- [ ] **Basic Auth** — protect routes or the entire server with username/password authentication

### 📁 File Serving
- [ ] **Directory listing** — auto-generate a browsable file index when no default file is found in a directory

### 🌐 Hosting
- [ ] **Virtual Hosts** — serve multiple sites from one server instance, routing by domain or subdomain

### 📋 Logging
- [ ] **File logging** — write request logs and errors to a persistent log file with timestamps

---

## Built With

- C# / .NET
- `System.Net.HttpListener` — built-in .NET HTTP server
- No external dependencies

---

## License

MIT — do whatever you want with it.
