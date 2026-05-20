using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleApp3.Classes;

namespace ConsoleApp3.Services
{
    internal class AdidasMembershipService
    {
        public async Task<string> GetMemberStatusAsync(string apiKey, string apiSecret, string memberId = "LEXGPSNFMLKLS2JA")
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | (SecurityProtocolType)12288;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            Console.WriteLine("\n=== STATUS: (HttpWebRequest - Forced GET Content-Type) ===");

            var brand = "ADI";
            var country = "CL";
            var url = $"https://mbs.qa.services.adidas.com/membership/{brand}/{country}/members/{memberId}/status";

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = GenerateSignature(apiKey, apiSecret, timestamp);

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                
                // HttpWebRequest permite poner ContentType en un GET
                request.ContentType = "application/json";
                request.Accept = "*/*";
                request.UserAgent = "PostmanRuntime/7.29.0";
                request.Headers.Add("x-api-key", apiKey);
                request.Headers.Add("x-signature", signature);
                request.Headers.Add("Instance-UID", "server-side");
                request.Headers.Add("Consumer-Type", "server-side");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Timeout = 15000;

                Console.WriteLine($"URL: {url}");

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var content = await reader.ReadToEndAsync();
                    Console.WriteLine($"Status Code: {(int)response.StatusCode} ({response.StatusCode})");

                    var status = JsonSerializer.Deserialize<MemberStatus>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return $"Éxito: Puntos Totales = {status.TotalPoints}, Tier = {status.TierId}";
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var errorContent = await reader.ReadToEndAsync();
                        return $"Fallo en la API Status ({(int)((HttpWebResponse)ex.Response).StatusCode}): {FormatJson(errorContent)}";
                    }
                }
                return $"Error en HttpWebRequest Status: {ex.Message}";
            }
        }

        public async Task<string> GetMemberRewardsAsync(string apiKey, string apiSecret, string memberId = "LEXGPSNFMLKLS2JA")
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | (SecurityProtocolType)12288;

            Console.WriteLine("\n=== REWARDS: (HttpWebRequest - Forced GET Content-Type) ===");

            var brand = "ADI";
            var country = "CL";
            var url = $"https://mbs.qa.services.adidas.com/membership/{brand}/{country}/members/{memberId}/rewards?rewardType=&metaDataFlag=false";

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = GenerateSignature(apiKey, apiSecret, timestamp);

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Accept = "*/*";
                request.UserAgent = "PostmanRuntime/7.29.0";
                request.Headers.Add("x-api-key", apiKey);
                request.Headers.Add("x-signature", signature);
                request.Headers.Add("Instance-UID", "server-side");
                request.Headers.Add("Consumer-Type", "server-side");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Timeout = 15000;

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var content = await reader.ReadToEndAsync();
                    Console.WriteLine($"Status Code: {(int)response.StatusCode} ({response.StatusCode})");

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var vaucherResponse = JsonSerializer.Deserialize<Vaucher>(content, options);

                    var activeVouchers = vaucherResponse.Rewards
                        .Where(r => r.RewardType == "VOUCHER" && r.Status == "ACTIVE")
                        .ToList();

                    return $"Éxito: Se encontraron {activeVouchers.Count} vouchers activos.";
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var errorContent = await reader.ReadToEndAsync();
                        return $"Fallo en la API Rewards ({(int)((HttpWebResponse)ex.Response).StatusCode}): {FormatJson(errorContent)}";
                    }
                }
                return $"Error en Rewards WebRequest: {ex.Message}";
            }
        }

        private string GenerateSignature(string apiKey, string apiSecret, long timestamp)
        {
            var input = $"{apiKey}{apiSecret}{timestamp}";

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hexString = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    hexString.Append(b.ToString("x2"));
                }
                return hexString.ToString();
            }
        }

        private string FormatJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "{}";
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
