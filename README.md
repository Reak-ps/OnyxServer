# ![OnyxServer Banner](banner.png)

<div align="center">

![.NET](https://img.shields.io/badge/.NET-6.0+-512BD4?style=for-the-badge\&logo=dotnet\&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge\&logo=csharp\&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-black?style=for-the-badge)
![Version](https://img.shields.io/badge/version-0.2.0-grey?style=for-the-badge)

[![Discord](https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge\&logo=discord\&logoColor=white)](https://discord.gg/yUvqZxzUKF)

</div>

---

A lightweight, high-performance static file server and reverse proxy built with **C#** and **.NET**.

Designed as a simple, zero-dependency alternative to Nginx for personal projects, self-hosted services, and lightweight deployments.

---

## ✨ Features

* 🚀 **Self-Contained Deployment** – Runs on Linux and Windows without requiring a .NET installation.
* 🔄 **Built-in Reverse Proxy** – Forward requests to local applications such as Gitea, Node.js apps, APIs, and more.
* ⚙️ **Automatic Configuration** – Missing configuration files are generated automatically on startup.
* 📡 **API Routing** – Serve dynamic JSON responses and access query parameters easily.
* 📊 **Structured Logging** – Log levels (`DEBUG`, `INFO`, `WARN`, `ERROR`) with optional file output.
* 📁 **Directory Listing** – Browse folders through a customizable directory template.
* 🛡️ **Security Protection** – Built-in protection against directory traversal attacks.
* 🌍 **Dynamic MIME Detection** – Correctly serves common file types automatically.
* 🔒 **HTTPS Support** – Optional SSL/TLS support for secure connections.
* 🪶 **Lightweight** – No external NuGet packages required.

---

# 🚀 Getting Started

## Option A — Standalone Linux Release

Perfect for VPS deployments, homelabs, and lightweight servers.

### 1. Download the latest release

Download `OnyxServer-linux-x64.zip` from the GitHub Releases page.

### 2. Extract the archive

```bash
unzip OnyxServer-linux-x64.zip
cd OnyxServer
```

### 3. Run the server

```bash
chmod +x OnyxServer
./OnyxServer
```

On first startup, OnyxServer automatically generates:

```text
ONYXSERVER.conf
server.log
```

---

## Option B — Build From Source

```bash
git clone https://github.com/Reak-ps/OnyxServer.git
cd OnyxServer
dotnet run
```

---

# ⚙️ Configuration

All settings are stored inside:

```text
ONYXSERVER.conf
```

Example configuration:

```properties
IP=0.0.0.0
PORT=8080

PUBLIC_FOLDER=Public/
SYSTEM_FOLDER=System/

DEFAULT_FILE=index.html

NOT_FOUND=404.html
FORBIDDEN=403.html
DIR_TEMPLATE=dir_template.html

# Logging
LOG_LEVEL=INFO
LOG_FILE=server.log
LOG_TO_CONSOLE=true

# SSL
SSL_ENABLED=false
HTTPS_PORT=8443

# Reverse Proxy
# PROXY:/git=http://localhost:3000
```

If the configuration file is missing, it will be recreated automatically with default values.

---

# 🔄 Reverse Proxy

Expose self-hosted applications behind a single entry point.

Example:

```properties
PROXY:/git=http://localhost:3000
```

Requests to:

```text
http://your-server:8080/git
```

are forwarded to:

```text
http://localhost:3000
```

This works great for:

* Gitea
* Node.js applications
* ASP.NET APIs
* Internal dashboards
* Self-hosted services

---

# 📁 Project Structure

```text
OnyxServer/
├── OnyxServer
├── ONYXSERVER.conf
├── server.log
│
├── Public/
│   └── index.html
│
└── System/
    ├── 404.html
    ├── 403.html
    └── dir_template.html
```

---

# 📊 Logging

Example output:

```text
[2026-06-16 21:04:12] [INFO] Server started. HTTP Port: 8080
[2026-06-16 21:04:12] [INFO] Reverse proxy active: /git -> http://localhost:3000
[2026-06-16 21:04:15] [INFO] [200] OK DELIVERED: /index.html
[2026-06-16 21:04:16] [WARN] [403] Forbidden /../../../etc/passwd
[2026-06-16 21:04:17] [INFO] [PROXY] GET /git/ -> http://localhost:3000/git/ (200)
```

---

# 🗺️ Roadmap

## Completed

* [x] Modular directory listing
* [x] Dedicated system folder for templates
* [x] Dynamic MIME type handling
* [x] Structured logging
* [x] Query parameter parsing
* [x] API endpoints with JSON responses
* [x] Reverse proxy support
* [x] HTTPS / SSL support
* [x] Linux self-contained builds

## Planned

* [ ] Virtual hosts
* [ ] Rate limiting
* [ ] Access control lists (ACL)
* [ ] Configuration hot reload
* [ ] WebSocket proxy support
* [] Compression (Gzip/Brotli)

---

# 🛠️ Built With

* System.Net.HttpListener
* System.Net.Http.HttpClient
* .NET 6+
* an extreme amount of patience

**No external NuGet packages. No unnecessary dependencies needed.**

---

# 📄 License

Released under the MIT License.

Use it, modify it, distribute it, or build on top of it — no restrictions beyond the MIT license terms.
