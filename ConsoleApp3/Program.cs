using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 🔐 Variables principales
            string baseUrl = GetBaseUrl();
            string endpoint = GetEndpoint();

            string apiKey = GetApiKey();
            string apiSecret = GetApiSecret();

            long timestamp = GetTimestamp();
            string signature = GenerateSignature(apiKey, apiSecret, timestamp);

            string bodyJson = BuildBody();

            var client = CreateClient(baseUrl);
            var request = CreateRequest(endpoint);

            AddHeaders(request, apiKey, signature, timestamp);
            AddBody(request, bodyJson);

            IRestResponse response = client.Execute(request);

            //var response = ExecuteRequest(client, request);

            //PrintResponse(response);

            Console.ReadLine(); // para que no cierre la consola
        }


        // =========================
        // 🔹 CONFIG / VARIABLES
        // =========================

        static string GetBaseUrl()
        {
            return "https://mbs.qa.services.adidas.com";
        }

        static string GetEndpoint()
        {
            return "/membership/lookup/acid";
        }

        static string GetApiKey()
        {
            return "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        }

        static string GetApiSecret()
        {
            return "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        }

        static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        


        static string GenerateSignature(string apiKey, string apiSecret, long timestamp)
        {
            // ⚠️ concatenación EXACTA (sin espacios)
            string raw = $"{apiKey}{apiSecret}{timestamp}";

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(raw);
                byte[] hash = sha256.ComputeHash(bytes);

                // convertir a HEX igual que CryptoJS
                var sb = new StringBuilder();
                foreach (var b in hash)
                    sb.Append(b.ToString("x2")); // minúscula

                return sb.ToString();
            }
        }

        // =========================
        // 🔹 BODY
        // =========================

        static string BuildBody()
        {
            var body = new
            {
                brand = "ADI",
                country = "CL",
                email = "santiago.gonzalez@externals.adidas.com"
            };

            return JsonSerializer.Serialize(body);
        }

        // =========================
        // 🔹 REQUEST
        // =========================

        static RestClient CreateClient(string baseUrl)
        {
            return new RestClient(baseUrl);
        }

        static RestRequest CreateRequest(string endpoint)
        {
            return new RestRequest(endpoint, Method.POST);
        }

        static void AddHeaders(RestRequest request, string apiKey, string signature, long timestamp)
        {
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("x-api-key", apiKey);
            request.AddHeader("x-signature", signature);

            // ⚠️ si la API lo requiere
            //request.AddHeader("x-timestamp", timestamp.ToString());

            request.AddHeader("Instance-UID", "server-side");
            request.AddHeader("Consumer-Type", "server-side");
        }

        static void AddBody(RestRequest request, string json)
        {
            request.AddParameter("application/json", json, ParameterType.RequestBody);
        }

        //static RestResponse ExecuteRequest(RestClient client, RestRequest request)
        //{
        //    return client.Execute(request); // síncrono
        //}

        // =========================
        // 🔹 RESPONSE
        // =========================

        static void PrintResponse(RestResponse response)
        {
            Console.WriteLine("Status: " + response.StatusCode);
            Console.WriteLine("Response:");
            Console.WriteLine(response.Content);
        }
    }
}
