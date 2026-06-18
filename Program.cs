using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Collections.Specialized;

namespace OnyxServer
{
    enum LogLevel { DEBUG = 0, INFO = 1, WARN = 2, ERROR = 3 }

    static class Logger
    {
        public static LogLevel MinLevel = LogLevel.INFO;
        public static bool LogToConsole = true;
        public static string LogFilePath = "server.log";

        public static void Log(LogLevel level, string message)
        {
            if (level < MinLevel) return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string formatted = $"[{timestamp}] [{level}] {message}";

            if (LogToConsole)
            {
                Console.WriteLine(formatted);
            }

            File.AppendAllText(LogFilePath, formatted + "\n");
        }

        public static void Debug(string message) => Log(LogLevel.DEBUG, message);
        public static void Info(string message) => Log(LogLevel.INFO, message);
        public static void Warn(string message) => Log(LogLevel.WARN, message);
        public static void Error(string message) => Log(LogLevel.ERROR, message);
    }

    class OnyxServer
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        private static string publicRoot;
        private static string systemRoot;
        private static string file;
        private static string notfound;
        private static string forb;
        private static string dir;
        private static Dictionary<string, string> proxyRoutes;
        private static Dictionary<string, Func<NameValueCollection, string>> apiRoutes;

        static async Task Main(string[] args)
        {
            string mainFolder = ResolveMainFolder();

            EnsureConfigExists(mainFolder + "ONYXSERVER.conf");
            Dictionary<string, string> config = LoadConfig(mainFolder + "ONYXSERVER.conf");

            string port = GetConfigValue(config, "PORT", "8080");
            string ip = GetConfigValue(config, "IP", "127.0.0.1");
            file = GetConfigValue(config, "DEFAULT_FILE", "index.html");
            notfound = GetConfigValue(config, "NOT_FOUND", "404.html");
            forb = GetConfigValue(config, "FORBIDDEN", "403.html");
            dir = GetConfigValue(config, "DIR_TEMPLATE", "dir_template.html");

            Logger.LogFilePath = mainFolder + GetConfigValue(config, "LOG_FILE", "server.log");
            Logger.LogToConsole = GetConfigValue(config, "LOG_TO_CONSOLE", "true")
                .Equals("true", StringComparison.OrdinalIgnoreCase);
            Logger.MinLevel = Enum.TryParse<LogLevel>(GetConfigValue(config, "LOG_LEVEL", "INFO"), true, out var parsedLevel)
                ? parsedLevel
                : LogLevel.INFO;

            bool sslEnabled = GetConfigValue(config, "SSL_ENABLED", "false")
                .Equals("true", StringComparison.OrdinalIgnoreCase);
            string httpsPort = GetConfigValue(config, "HTTPS_PORT", "8443");

            string publicFolderConfig = GetConfigValue(config, "PUBLIC_FOLDER", "Public/");
            string systemFolderConfig = GetConfigValue(config, "SYSTEM_FOLDER", "System/");

            publicRoot = NormalizeFolder(
                Path.IsPathRooted(publicFolderConfig) ? publicFolderConfig : $"{mainFolder}{publicFolderConfig}");
            systemRoot = NormalizeFolder(
                Path.IsPathRooted(systemFolderConfig) ? systemFolderConfig : $"{mainFolder}{systemFolderConfig}");

            if (!Directory.Exists(publicRoot))
            {
                Logger.Error($"PUBLIC_FOLDER does not exist: {publicRoot}. Please correct this in ONYXSERVER.conf.");
                Environment.Exit(1);
            }
            if (!Directory.Exists(systemRoot))
            {
                Logger.Error($"SYSTEM_FOLDER does not exist: {systemRoot}. Please correct this in ONYXSERVER.conf.");
                Environment.Exit(1);
            }

            proxyRoutes = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kvp in config)
            {
                if (kvp.Key.StartsWith("PROXY:", StringComparison.OrdinalIgnoreCase))
                {
                    string proxyPath = kvp.Key.Substring("PROXY:".Length);
                    proxyRoutes[proxyPath] = kvp.Value;
                }
            }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{ip}:{port}/");

            if (sslEnabled)
            {
                listener.Prefixes.Add($"https://{ip}:{httpsPort}/");
            }
            
            try
            {
                listener.Start();
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 13 || ex.ErrorCode == 5)
            {
                Logger.Error($"Port not authorized {port}. On Linux, ports below 1024 require root privileges. " +
                             "Solution: Use a port > 1024, or run 'sudo setcap CAP_NET_BIND_SERVICE=+eip <path-to-exe>'");
                Environment.Exit(1);
                return;
            }
            catch (HttpListenerException ex)
            {
                Logger.Error($"Server could not be started: {ex.Message}");
                Environment.Exit(1);
                return;
            }

            Console.CancelKeyPress += (sender, e) =>
            {
                Logger.Info("Server is shutting down (Strg+C)...");
                listener.Stop();
                listener.Close();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Logger.Info("Server is shutting down (SIGTERM)...");
                listener.Stop();
                listener.Close();
            };

            Logger.Info($"Server started. HTTP Port: {port}" + (sslEnabled ? $", HTTPS Port: {httpsPort}" : ""));
            
            apiRoutes = new Dictionary<string, Func<NameValueCollection, string>>();

            apiRoutes.Add("/api/stats", (query) =>
            {
                bool pretty = query["format"] == "pretty";

                if (pretty)
                {
                    return $$"""
                             {
                               "status": "online",
                               "version": "0.2.0",
                               "time": "{{DateTime.Now}}"
                             }
                             """;
                }
                else
                {
                    return $$"""{"status":"online","version":"0.2.0","time":"{{DateTime.Now}}"}""";
                }
            });

            apiRoutes.Add("/api/system", (query) =>
            {
                return $$"""
                         {
                           "os": "{{Environment.OSVersion}}",
                           "cpu_cores": {{Environment.ProcessorCount}},
                           "machine_name": "{{Environment.MachineName}}"
                         }
                         """;
            });

            // new asynchron 
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                
                _ = Task.Run(() => HandleClientAsync(context));
            }
        } // end of main methode

        // new thing for multi threading
        static async Task HandleClientAsync(HttpListenerContext context)
        {
            try
            {
                Uri requestUrl = context.Request.Url;
                string requestedfile = requestUrl.AbsolutePath;
                NameValueCollection queryParams = HttpUtility.ParseQueryString(requestUrl.Query);

                HttpListenerResponse response = context.Response;

                if (requestedfile == "/")
                {
                    requestedfile = $"/{file}";
                }

                string fullpath = $"{publicRoot}{requestedfile}";
                if (requestedfile.Contains(".."))
                {
                    Logger.Warn($"{GetStatusLabel(403)} {fullpath}");
                    string forbiddenerror = $"{systemRoot}{forb}";

                    string extension = Path.GetExtension(forb);
                    response.ContentType = GetMimeType(extension);
                    byte[] buffer = File.ReadAllBytes(forbiddenerror);

                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 403;

                    using (System.IO.Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                    }
                    return;
                }

                if (apiRoutes.ContainsKey(requestedfile))
                {
                    string jsonResponse = apiRoutes[requestedfile](queryParams);

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;

                    using (System.IO.Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                    }
                    Logger.Debug($"API route served: {requestedfile}");
                    return;
                }
                else if (requestedfile.StartsWith("/api/"))
                {
                    string errorJson = """
                                       {
                                         "error": "Not Found",
                                         "message": "This API route does not exist..."
                                       }
                                       """;
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(errorJson);
                    response.ContentType = "application/json";
                    response.StatusCode = 404;
                    response.ContentLength64 = buffer.Length;

                    using (System.IO.Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                    }
                    Logger.Warn($"Unknown API route requested: {requestedfile}");
                    return;
                }

                string matchedProxyPath = null;
                foreach (KeyValuePair<string, string> proxy in proxyRoutes)
                {
                    if (requestedfile.StartsWith(proxy.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedProxyPath = proxy.Key;
                        break;
                    }
                }

                if (matchedProxyPath != null)
                {
                    string targetBase = proxyRoutes[matchedProxyPath];
                    string remainingPath = requestedfile.Substring(matchedProxyPath.Length);
                    string targetUrl = $"{targetBase}{remainingPath}{requestUrl.Query}";

                    await HandleProxyRequest(context, targetUrl);
                    return;
                }

                if (Directory.Exists(fullpath))
                {
                    string[] files = Directory.GetFiles(fullpath);
                    string listItems = "";

                    foreach (string singleFile in files)
                    {
                        string name = Path.GetFileName(singleFile);
                        listItems += $"<li><a href='{requestedfile}/{name}'>{name}</a></li>\n";
                    }

                    string templatePath = $"{systemRoot}{dir}";
                    string html = File.ReadAllText(templatePath);
                    html = html.Replace("###FILE_LIST###", listItems);

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);

                    response.ContentType = "text/html; charset=utf-8";
                    response.ContentLength64 = buffer.Length;

                    using (System.IO.Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                    }
                    Logger.Info($"[DIR] LISTED: {requestedfile}");
                }
                else
                {
                    string extension = Path.GetExtension(fullpath);
                    response.ContentType = GetMimeType(extension);
                    byte[] buffer = File.ReadAllBytes(fullpath);

                    response.ContentLength64 = buffer.Length;
                    using (System.IO.Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{GetStatusLabel(404)} NOT FOUND: {context.Request.Url.AbsolutePath} ({ex.Message})");
                string errorpath = $"{systemRoot}{notfound}";

                string extension = Path.GetExtension(notfound);
                context.Response.ContentType = GetMimeType(extension);
                byte[] buffer = File.ReadAllBytes(errorpath);

                context.Response.ContentLength64 = buffer.Length;
                context.Response.StatusCode = 404;
                using (System.IO.Stream output = context.Response.OutputStream)
                {
                    await output.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            finally
            {
                // close the connection doesn't matter what happens
                context.Response.Close();
            }
        }

        static async Task HandleProxyRequest(HttpListenerContext context, string targetUrl)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                var proxyRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), targetUrl);

                if (request.HasEntityBody)
                {
                    using MemoryStream memoryStream = new MemoryStream();
                    await request.InputStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    proxyRequest.Content = new StreamContent(memoryStream);

                    if (!string.IsNullOrEmpty(request.ContentType))
                    {
                        proxyRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
                    }
                }

                foreach (string headerKey in request.Headers.AllKeys)
                {
                    if (headerKey.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
                    if (headerKey.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)) continue;
                    if (headerKey.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;

                    try
                    {
                        proxyRequest.Headers.TryAddWithoutValidation(headerKey, request.Headers[headerKey]);
                    }
                    catch { }
                }

                HttpResponseMessage proxyResponse = await httpClient.SendAsync(proxyRequest);

                response.StatusCode = (int)proxyResponse.StatusCode;
                response.ContentType = proxyResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                byte[] responseBytes = await proxyResponse.Content.ReadAsByteArrayAsync();
                response.ContentLength64 = responseBytes.Length;

                using System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(responseBytes, 0, responseBytes.Length);

                Logger.Info($"[PROXY] {request.HttpMethod} {request.RawUrl} -> {targetUrl} ({(int)proxyResponse.StatusCode})");
            }
            catch (Exception ex)
            {
                Logger.Error($"[PROXY] Error forwarding to {targetUrl}: {ex.Message}");
                response.StatusCode = 502;
                byte[] errorBytes = System.Text.Encoding.UTF8.GetBytes("502 Bad Gateway: Proxy target unreachable.");
                response.ContentType = "text/plain";
                response.ContentLength64 = errorBytes.Length;
                using System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(errorBytes, 0, errorBytes.Length);
            }
        }

        static string ResolveMainFolder()
        {
            string envOverride = Environment.GetEnvironmentVariable("ONYXSERVER_HOME");
            if (!string.IsNullOrEmpty(envOverride))
            {
                return NormalizeFolder(envOverride);
            }
            string baseDir = Path.GetDirectoryName(Environment.ProcessPath);
            
            if (string.IsNullOrEmpty(baseDir)) 
            {
                baseDir = AppContext.BaseDirectory;
            }
            if (baseDir.Contains("bin") && (baseDir.Contains("Debug") || baseDir.Contains("Release")))
            {
                return NormalizeFolder(Path.GetFullPath(Path.Combine(baseDir, "../../../")));
            }
            
            return NormalizeFolder(baseDir);
        }

        static void EnsureConfigExists(string path)
        {
            if (File.Exists(path)) return;

            string defaultConfig = """
            # ====================================================
            # OnyxServer Configuration
            # Lines starting with # are comments and are ignored
            # ====================================================
            
            # IP and port the server listens on
            IP=127.0.0.1
            PORT=8080

            # Folder served as a website
            PUBLIC_FOLDER=Public/

            # Folder with error pages & templates
            SYSTEM_FOLDER=System/

            # File loaded when someone visits the main page (/)
            DEFAULT_FILE=index.html

            # Error pages
            NOT_FOUND=404.html
            FORBIDDEN=403.html
            DIR_TEMPLATE=dir_template.html

            # Logging
            LOG_LEVEL=INFO
            LOG_FILE=server.log
            LOG_TO_CONSOLE=true

            # SSL/HTTPS
            SSL_ENABLED=false
            HTTPS_PORT=8443
            """;

            File.WriteAllText(path, defaultConfig);
        }

        static string NormalizeFolder(string path)
        {
            if (!path.EndsWith("/") && !path.EndsWith("\\"))
            {
                path += "/";
            }
            return path;
        }

        static Dictionary<string, string> LoadConfig(string path)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                string[] parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                {
                    result[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return result;
        }

        static string GetConfigValue(Dictionary<string, string> config, string key, string fallback)
        {
            return config.TryGetValue(key, out string value) ? value : fallback;
        }

        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".html", "text/html; charset=utf-8" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".txt", "text/plain; charset=utf-8" }
        };

        public static string GetMimeType(string extension)
        {
            if (MimeTypes.TryGetValue(extension, out string mimeType))
            {
                return mimeType;
            }
            return "application/octet-stream";
        }

        static string GetStatusLabel(int statusCode)
        {
            switch (statusCode)
            {
                case 200: return "[200] OK";
                case 403: return "[403] Forbidden";
                case 404: return "[404] Not Found";
                default: return "[SERVER] HTTP STATUS";
            }
        }
    }
}