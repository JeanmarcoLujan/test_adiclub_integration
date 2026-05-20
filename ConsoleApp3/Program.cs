using ConsoleApp3.Classes;
using ConsoleApp3.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    internal class Program
    {
        public static string signatureBK = "";
        static async Task Main(string[] args)
        {
            var apiKey = "RRA7tuiGQpI2yoNnLQTFcWhgwRfy3nN4";
            var apiSecret = "Sx7tAGWUjh8GZc4tNfl3Aoe3SQFiVriz";

            try 
            {
                var service = new AdidasMembershipService();

                Console.WriteLine("--- Prueba 1: Lookup ---");
                //var resultLookup = await Lookup(apiKey, apiSecret);
                //Console.WriteLine(resultLookup);

                Console.WriteLine("\n--- Prueba 2: GetMemberStatus (HttpClient) ---");
                var resultStatus = await service.GetMemberStatusAsync(apiKey, apiSecret);
                Console.WriteLine(resultStatus);

                Console.WriteLine("\n--- Prueba 3: GetMemberRewards (HttpClient) ---");
                var resultRewards = await service.GetMemberRewardsAsync(apiKey, apiSecret);
                Console.WriteLine(resultRewards);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError inesperado: {ex.Message}");
            }

            Console.WriteLine("\nPresione cualquier tecla para salir...");
        }

        static async Task<string> GetMemberStatus(string apiKey, string apiSecret)
        {
            Console.WriteLine("\n=== GET: Member Status (RestSharp) ===");

            var brand = "ADI";
            var country = "CL";
            var memberId = "OLFCIWKWUUEDWV3F";
            
            var url = $"https://mbs.qa.services.adidas.com/membership/{brand}/{country}/members/{memberId}/status";

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = GenerateSignature(apiKey, apiSecret, timestamp);

            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("x-api-key", apiKey);
            request.AddHeader("x-signature", signature);
            request.AddHeader("Instance-UID", "server-side");
            request.AddHeader("Consumer-Type", "server-side");

            Console.WriteLine($"Timestamp: {timestamp}");
            Console.WriteLine($"URL: {url}");

            var response = await client.ExecuteAsync(request);

            Console.WriteLine($"Status Code: {(int)response.StatusCode} ({response.StatusCode})");
            
            if (response.IsSuccessful)
            {
                try 
                {
                    var status = JsonSerializer.Deserialize<MemberStatus>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return $"Éxito: Puntos Totales = {status.TotalPoints}, Tier = {status.TierId}";
                }
                catch (Exception ex)
                {
                    return $"Error al deserializar: {ex.Message}\nRaw JSON: {response.Content}";
                }
            }
            else
            {
                return $"Fallo en la API: {FormatJson(response.Content)}";
            }
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
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Instance-UID", "server-side");
            request.Headers.Add("Consumer-Type", "server-side");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("x-signature", signature);
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");

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

        static long GetGermanyTimestamp()
        {
            // Zona horaria de Alemania (Europe/Berlin)
            TimeZoneInfo germanyZone;

            try
            {
                // Para Linux/Mac
                germanyZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            }
            catch (TimeZoneNotFoundException)
            {
                // Para Windows
                germanyZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            }

            // Obtener la hora actual en Alemania
            DateTime germanyTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, germanyZone);

            // Convertir a Unix timestamp (segundos desde 1970-01-01 UTC)
            // IMPORTANTE: El timestamp DEBE estar en UTC, no en hora local de Alemania
            // porque Unix timestamp es siempre UTC
            long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            // Mostrar información de depuración
            Console.WriteLine($"\n[DEBUG] Hora UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"[DEBUG] Hora Alemania: {germanyTime:yyyy-MM-dd HH:mm:ss} ({(IsDaylightSavingTime(germanyZone) ? "Verano UTC+2" : "Invierno UTC+1")})");
            Console.WriteLine($"[DEBUG] Hora Perú: {TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetPeruZone()):yyyy-MM-dd HH:mm:ss} (UTC-5)");
            Console.WriteLine($"[DEBUG] Timestamp UTC: {timestamp}");

            return timestamp;
        }

        static TimeZoneInfo GetPeruZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Lima");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            }
        }

        static bool IsDaylightSavingTime(TimeZoneInfo zone)
        {
            return zone.IsDaylightSavingTime(DateTime.UtcNow);
        }


    }
}
