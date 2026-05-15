using ConsoleApp3.Classes;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
//using System.Net.Http;

namespace ConsoleApp3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {


            var apiKey = "RRA7tuiGQpI2yoNnLQTFcWhgwRfy3nN4";
            var apiSecret = "Sx7tAGWUjh8GZc4tNfl3Aoe3SQFiVriz";

            var resultStatus = await GetMemberStatus(apiKey, apiSecret);
            Console.WriteLine(resultStatus);

            var result = await GetMemberRewards(apiKey, apiSecret);
            Console.WriteLine(result);

            var resultLookup = await Lookup(apiKey, apiSecret);
            Console.WriteLine(resultLookup);



        }

        static async Task<string> GetMemberStatus(string apiKey, string apiSecret)
        {
            Console.WriteLine("\n=== GET: Member Status ===\n");

            var brand = "ADI";
            var country = "CL";
            var memberId = "LEXGPSNFMLKLS2JA";

            var url = $"https://mbs.qa.services.adidas.com/membership/{brand}/{country}/members/{memberId}/status";

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = GenerateSignature(apiKey, apiSecret, timestamp);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Headers (incluyendo Cookie como en el ejemplo)
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Instance-UID", "server-side");
            request.Headers.Add("Consumer-Type", "server-side");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("x-signature", signature);
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            //request.Headers.Add("Pragma", "X-Akamai-Session-Info,akamai-x-cache-on, akamai-x-cache-remote-on, akamai-x-check-cacheable, akamai-x-get-cache-key, akamai-x-get-nonces, akamai-x-get-ssl-client-session-id, akamai-x-get-true-cache-key, akamai-x-serial-no, akamai-x-get-request-id");



            Console.WriteLine($"Timestamp: {timestamp}");
            Console.WriteLine($"Signature: {signature}");
            Console.WriteLine($"URL: {url}\n");

            var client = new HttpClient();
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Response: {FormatJson(result)}");

            return $"Status: {(int)response.StatusCode}\nResponse: {FormatJson(result)}";
        }

        static async Task<string> GetMemberRewards(string apiKey, string apiSecret)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = GenerateSignature(apiKey, apiSecret, timestamp);

            var url = "https://mbs.qa.services.adidas.com/membership/ADI/CL/members/LEXGPSNFMLKLS2JA/rewards?rewardType=&metaDataFlag=false";

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Headers
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("x-signature", signature);
            request.Headers.Add("Instance-UID", "server-side");
            request.Headers.Add("Consumer-Type", "server-side");
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            //request.Headers.Add("Content-Type", "application/json");
            //request.Headers.Add("Pragma", "X-Akamai-Session-Info,akamai-x-cache-on, akamai-x-cache-remote-on, akamai-x-check-cacheable, akamai-x-get-cache-key, akamai-x-get-nonces, akamai-x-get-ssl-client-session-id, akamai-x-get-true-cache-key, akamai-x-serial-no, akamai-x-get-request-id");

            Console.WriteLine($"Timestamp: {timestamp}");
            Console.WriteLine($"Signature: {signature}");
            Console.WriteLine($"URL: {url}\n");

            var response = await client.SendAsync(request);


            string rr = "";
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Vaucher response1 = JsonSerializer.Deserialize<Vaucher>(result, options);


                var filteredRewards = response1.Rewards
                .Where(r =>
                    r.RewardType == "VOUCHER" &&
                    r.Status == "ACTIVE" 
                )
                .ToList();

                rr = JsonSerializer.Serialize(filteredRewards, options);

            }
            else
            {
                rr = "se ha producido un error al consultar en el api " + response.StatusCode.ToString();
            }




             return $"Status: {(int)response.StatusCode}\nResponse: {FormatJson(rr)}";
        }

        static async Task<string> Lookup(string apiKey, string apiSecret)
        {
            
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = GenerateSignature(apiKey, apiSecret, timestamp);


            // Crear request
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://mbs.qa.services.adidas.com/membership/lookup/acid");

            // Headers del request
            request.Headers.Add("Instance-UID", "server-side");
            request.Headers.Add("Consumer-Type", "server-side");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("x-signature", signature);

            // Body
            var body = new { brand = "ADI", country = "CL", email = "santiago.gonzalez@externals.adidas.com" };
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Enviar
            var client = new HttpClient();
            var response = await client.SendAsync(request);
            var status = response.StatusCode;
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {result}");

            return $"Status: {(int)response.StatusCode}\nResponse: {FormatJson(result)}";
        }


        static string GenerateSignature(string apiKey, string apiSecret, long timestamp)
        {
            // Concatenar: apiKey + apiSecret + timestamp
            var input = $"{apiKey}{apiSecret}{timestamp}";

            // Calcular SHA256
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Método compatible con todas las versiones de .NET
                var hexString = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    hexString.Append(b.ToString("x2"));
                }

                return hexString.ToString();
            }
        }

        static string FormatJson(string json)
        {
            try
            {
                var obj = JsonSerializer.Deserialize<object>(json);
                return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json;
            }
        }


    }
}
