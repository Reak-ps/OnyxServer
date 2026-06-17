using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace OnyxServer
{
    class OnyxServer
    {
        static async Task Main(string[] args)
        {
            // WE ARE PLAYIN THE JOKER GO 3 FOLDERS OUT TO GO TO THE MAIN FOLDER
            string mainFolder = "../../../";

            // LOAD CONFIG FROM THE MAIN FOLDER
            string[] config = File.ReadAllLines(mainFolder + "ONYXSERVER.conf");
            string[] portparts = config[0].Split('=');
            string port = portparts[1];
            string[] ipparts = config[1].Split('=');
            string ip = ipparts[1];
            string[] fileparts = config[3].Split('=');
            string file = fileparts[1];
            string[] notfoundparts = config[4].Split('=');
            string notfound = notfoundparts[1];
            string[] forbidden = config[5].Split('=');
            string forb = forbidden[1];
            string[] dircontents = config[6].Split('=');
            string dir = dircontents[1];
            
            string publicRoot = $"{mainFolder}Public/";
            string systemRoot = $"{mainFolder}System/";
            
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{ip}:{port}/");
            listener.Start();
            Console.WriteLine($"Listening on port {port}");
            Dictionary<string, Func<string>> apiRoutes = new Dictionary<string, Func<string>>();
            
            apiRoutes.Add("/api/stats", () => 
            {
                return $$"""
                         {
                           "status": "online",
                           "version": "0.2.0,
                           "time": "{{DateTime.Now}}"
                         }
                         """;
            });
            
            apiRoutes.Add("/api/system", () => 
            {
                return $$"""
                         {
                           "os": "{{Environment.OSVersion}}",
                           "cpu_cores": {{Environment.ProcessorCount}},
                           "machine_name": "{{Environment.MachineName}}"
                         }
                         """;
            });
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                string requestedfile = context.Request.RawUrl;
                HttpListenerResponse response = context.Response;
                
                if (requestedfile == "/")
                {
                    requestedfile = $"/{file}";
                }

                // This is for safety and putting everything togheter
                string fullpath = $"{publicRoot}{requestedfile}";
                if (requestedfile.Contains(".."))
                {
                    string errorMessage = $"[{DateTime.Now}] {GetStatusLabel(403)} {fullpath}";
                    Console.WriteLine(errorMessage);
                    File.AppendAllText(mainFolder + "server.log", errorMessage + "\n");    
                    string forbiddenerror = $"{systemRoot}{forb}";
                    
                    string extension = Path.GetExtension(forb);
                    response.ContentType = GetMimeType(extension);
                    byte[] buffer = File.ReadAllBytes(forbiddenerror);
                    
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 403;
                    
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    
                    response.Close();
                    continue;
                }

                if (apiRoutes.ContainsKey(requestedfile))
                {
                    string jsonResponse = apiRoutes[requestedfile]();
                    
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();

                    continue;
                }
                else if (requestedfile.StartsWith("/api/"))
                {
                    string errorJson = """
                                       {
                                         "error": "Not Found",
                                         "message": "Diese API-Route existiert nicht."
                                       }
                                       """;
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(errorJson);
                    response.ContentType = "application/json";
                    response.StatusCode = 404;
                    response.ContentLength64 = buffer.Length;
                    
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();

                    continue;
                }

try 
                {
                    // IF IT IS A DIR SHOW THE FOLDERCONTENTS
                    if (Directory.Exists(fullpath))
                    {
                        string[] files = Directory.GetFiles(fullpath);
                        string listItems = "";
    
                        foreach (string singleFile in files)
                        {
                            string name = Path.GetFileName(singleFile);
                            listItems += $"<li><a href='{requestedfile}/{name}'>{name}</a></li>\n";
                        }
    
                        // new magical things happen here 
                        string templatePath = $"{systemRoot}{dir}";
                        string html = File.ReadAllText(templatePath);
                        html = html.Replace("###FILE_LIST###", listItems);
    
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
    
                        response.ContentType = "text/html; charset=utf-8";
                        response.ContentLength64 = buffer.Length;
    
                        System.IO.Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
    
                        string logMessage = $"[{DateTime.Now}] [DIR] LISTED: {requestedfile}";
                        Console.WriteLine(logMessage);
                        File.AppendAllText(mainFolder + "server.log", logMessage + "\n");
                    }
                    // IF IT IS A FILE SEND  IT TO THE FCKING BROWSER
                    else
                    {
                        string extension = Path.GetExtension(fullpath);
                        response.ContentType = GetMimeType(extension);
                        byte[] buffer = File.ReadAllBytes(fullpath);
                        
                        response.ContentLength64 = buffer.Length;
                        System.IO.Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        
                        string logMessage = $"[{DateTime.Now}] {GetStatusLabel(200)} DELIVERED: {requestedfile}";
                        Console.WriteLine(logMessage);
                        File.AppendAllText(mainFolder + "server.log", logMessage + "\n");   
                    }
                } 

                catch (Exception ex)
                {
                    string errorMessage = $"[{DateTime.Now}] {GetStatusLabel(404)} NOT FOUND: {fullpath}";
                    Console.WriteLine(errorMessage);
                    File.AppendAllText(mainFolder + "server.log", errorMessage + "\n");    
                    string errorpath = $"{systemRoot}{notfound}";
                    
                    string extension = Path.GetExtension(notfound);
                    response.ContentType = GetMimeType(extension);
                    byte[] buffer = File.ReadAllBytes(errorpath);
                    
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 404; // HTTP Code for "Not Found"
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0 , buffer.Length);
                    output.Close();
                    
                    response.Close();
                }
            }
        }
        
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".html", "text/html; charset=utf-8" },
            { ".htm", "text/html; charset=utf-8" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" },
            { ".svg", "image/svg+xml" },
            { ".ico", "image/x-icon" },
            { ".txt", "text/plain; charset=utf-8" },
            { ".mp4", "video/mp4" },
            { ".pdf", "application/pdf" },
            { ".zip", "application/zip" }
        };

        public static string GetMimeType(string extension)
        {
            if (MimeTypes.TryGetValue(extension, out string mimeType))
            {
                return mimeType;
            }
    
            // Standard for not known types
            return "application/octet-stream"; 
        }

        static string GetStatusLabel(int statusCode)
        {
            switch (statusCode)
            {
                case 200: return "[200] OK";
                case 400: return "[400] Bad Request";
                case 404: return "[404] Not Found";
                case 403: return "[403] Forbidden";
                case 500: return "[500] OH OH THE SERVER IS NOT FEELING WELL (INTERNAL SERVER ERROR)";
                default: return "[SERVER] I DONT EVEN KNOW WHAT THIS IS NOW";
                
            }
        }
    }
}