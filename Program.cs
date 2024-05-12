// See https://aka.ms/new-console-template for more information
using System.Net;

Console.WriteLine("Hello, World!");

var servidorHttp = new ServidorHttp();

using (HttpListener listener = new HttpListener())
        {
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();

            Console.WriteLine("Aguardando solicitação...");

            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;

            if (request.HttpMethod == "POST" && request.HasEntityBody)
            {
                using (Stream body = request.InputStream)
                {
                    using (StreamReader reader = new StreamReader(body))
                    {
                        string formData = reader.ReadToEnd();
                        Console.WriteLine("Dados recebidos:");
                        Console.WriteLine(formData);
                    }
                }
            }
            else
            {
                Console.WriteLine("Método de requisição ou corpo inválido.");
            }

            listener.Stop();
        }