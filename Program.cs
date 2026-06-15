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
            string[] portparts = config[0].Split("=");
            string port = portparts[1];
            string[] ipparts = config[1].Split("=");
            string ip = ipparts[1];
            string[] folderparts = config[2].Split('=');
            string folder = folderparts[1];
            string[] fileparts = config[3].Split('=');
            string file = fileparts[1];

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{ip}:{port}/");
            listener.Start();
            Console.WriteLine($"Listening on port {port}");
            
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                string requestedfile = context.Request.RawUrl;
                HttpListenerResponse response = context.Response;
                
                if (requestedfile == "/")
                {
                    requestedfile = $"/{file}";
                }

                // STICKY SITUATIONS ARE NEEDED STICK EVERTHING TOGEHTER
                string fullpath = $"{mainFolder}{folder}{requestedfile}";

                try 
                {
                    // TRY TO READ AND SEND THE FILE
                    string extension = Path.GetExtension(fullpath);
                    response.ContentType = GetMimeType(extension);
                    byte[] buffer = File.ReadAllBytes(fullpath);

                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    
                    output.Close();
                    Console.WriteLine($"[OKAY] DELIVERED: {requestedfile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[INTERGALCTIC ERROR] FILE NOT FOUND: {fullpath}");
                    response.StatusCode = 404; // HTTP Code für "Not Found"
                    response.Close();
                }
            }
        }
        
        static string GetMimeType(string fileEnding)
        {
            switch (fileEnding.ToLower())
            {
                case ".html": return "text/html; charset=utf-8";
                case ".css":  return "text/css";
                case ".js":   return "application/javascript";
                case ".png":  return "image/png";
                case ".jpg":  return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                default:      return "application/octet-stream"; // STANDART NONE FILES
            }
        }
    }
}