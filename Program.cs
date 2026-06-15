using System;
using System.IO.Enumeration;
using System.Net;
using System.Threading.Tasks;

namespace OnyxServer
{
    class OnyxServer
    {
        static async Task Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Listening on port 8080");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerResponse response = context.Response;
                
                string responseString = "<html><body><h1>Hej!</h1></body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                
                output.Close();
                Console.WriteLine("Request was successfully Satisfied");
            }
        }
    }
}