
```
![OnyxServer Banner](banner.png)

<div align="center">

![.NET](https://img.shields.io/badge/.NET-6.0+-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-black?style=for-the-badge)
![Version](https://img.shields.io/badge/version-0.2.0-grey?style=for-the-badge)
[![Discord](https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/yUvqZxzUKF)

</div>

---

A lightweight, lightning-fast static file & proxy server built with C# and .NET. Designed as a zero-dependency Nginx alternative. Just drop in the executable, configure your routes, and serve.

---

## ✨ Features

- **Zero Dependencies (Self-Contained):** Runs natively on Linux and Windows without installing .NET!
- **Reverse Proxy:** Built-in proxy to forward requests to other local apps (e.g., Gitea, Node.js).
- **Auto-Configuring:** Missing the config file? OnyxServer generates a fresh one on startup.
- **Smart API Routing:** Serve dynamic JSON data (e.g., `/api/system`) with query parameter parsing.
- **Advanced Logging:** Professional, leveled logging (`DEBUG`, `INFO`, `WARN`, `ERROR`) to console and file.
- **Dynamic MIME Types:** Automatically serves HTML, CSS, JS, PNG, JPG, MP4, and more correctly.
- **Directory Listing:** Browse folder contents directly in the browser with a customizable template.
- **Security First:** Directory traversal protection blocks `../` attacks automatically.
- **SSL / HTTPS Support:** Built-in option to serve over encrypted connections.

---

## 🚀 Getting Started

OnyxServer is built to be portable. You don't need to compile anything if you just want to run it!

### Option A: The Quick Way (Linux Standalone)
Perfect for running OnyxServer on a VPS or home server.

1. Download the latest `OnyxServer-linux-x64.zip` from the Releases page.
2. Extract the archive (it contains the `OnyxServer` executable and the `Public`/`System` folders).
3. Open your terminal, make it executable, and run it:
   ```bash
   chmod +x OnyxServer
   ./OnyxServer

```
*(Note: On the first run, it will automatically generate an ONYXSERVER.conf file for you to edit!)*
### Option B: For Developers (Build from Source)
If you want to modify the C# code:
```bash
git clone [https://github.com/your-username/OnyxServer.git](https://github.com/your-username/OnyxServer.git)
cd OnyxServer
dotnet run

```
## ⚙️ Configuration
OnyxServer reads its settings from ONYXSERVER.conf in the project root. If you delete it, the server will recreate it with default values.
```properties
IP=0.0.0.0
PORT=8080
PUBLIC_FOLDER=Public/
SYSTEM_FOLDER=System/
DEFAULT_FILE=index.html
NOT_FOUND=404.html
FORBIDDEN=403.html
DIR_TEMPLATE=dir_template.html

# Logging (DEBUG, INFO, WARN, ERROR)
LOG_LEVEL=INFO
LOG_FILE=server.log
LOG_TO_CONSOLE=true

# SSL
SSL_ENABLED=false
HTTPS_PORT=8443

# Reverse Proxy (Uncomment to use)
# PROXY:/gitea=http://localhost:3000

```
### 🔄 Using the Reverse Proxy
OnyxServer can act as a gateway for your other self-hosted apps. Want to host Gitea alongside your static site? Just add this to your config:
PROXY:/git=http://localhost:3000
Now, navigating to your-ip:8080/git will seamlessly serve your Gitea instance!
## 📁 Project Structure
```text
OnyxServer_Release/
├── OnyxServer             ← The self-contained Linux executable
├── ONYXSERVER.conf        ← Auto-generated config file
├── server.log             ← Auto-generated log file
├── Public/                ← Drop your website files here!
│   └── index.html
└── System/                ← Internal templates (protected)
    ├── 404.html
    ├── 403.html
    └── dir_template.html

```
## 📊 Professional Logging
Every request, proxy forward, and error is logged cleanly with levels and timestamps:
```text
[2026-06-16 21:04:12] [INFO] Server started. HTTP Port: 8080
[2026-06-16 21:04:12] [INFO] Reverse proxy active: /gitea -> http://localhost:3000
[2026-06-16 21:04:15] [INFO] [200] OK DELIVERED: /index.html
[2026-06-16 21:04:16] [WARN] [403] Forbidden /../../../etc/passwd
[2026-06-16 21:04:17] [INFO] [PROXY] GET /gitea/ -> http://localhost:3000/gitea/ (200)

```
## 🗺️ Roadmap (v0.2.0 Status)
### 🔧 Core Improvements
 * [x] **Modular folder contents** — cleaner, more flexible directory listing system
 * [x] **Error pages in separate folder** — move 404/403 pages out of the public directory
 * [x] **Dynamic MIME types** — Dictionary-based, fast MIME type detection
 * [x] **Professional logging** — structured, leveled log output (DEBUG, INFO, WARN, ERROR)
 * [x] **Query parameter parsing** — read and handle URL query strings automatically
 * [x] **System files vs. public files** — separate internal server files from served content
### 🌐 API & Proxy
 * [x] **API endpoints & JSON** — serve JSON responses from defined endpoints (/api/system)
 * [x] **Testing Gitea / Self-Hosted apps** — Fully working Reverse Proxy via HttpClient
### 🔒 Security & Platform
 * [x] **SSL / HTTPS** — serve over TLS with certificate configuration
 * [x] **Linux support** — Graceful shutdown & Self-Contained single-file publishing
## 🛠️ Built With
 * System.Net.HttpListener — core HTTP stack
 * System.Net.Http.HttpClient — Proxy engine
 * **Zero bloat. No external Nuget packages required.**
## 📄 License
MIT — do whatever you want with it.
```

```
