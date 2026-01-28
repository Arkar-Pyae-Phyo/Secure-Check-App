using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureCheckApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://mse.mfu.ac.th";

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<ApiResponse?> UploadCodeAsync(string code, string ipv4)
        {
            try
            {
                var requestData = new
                {
                    code = code,
                    ipv4 = ipv4
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ApiBaseUrl}/api/access/token", content);
                
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var result = JsonSerializer.Deserialize<ApiResponse>(responseBody, options);
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return null;
            }
        }
    }

    public class ApiResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        
        [JsonPropertyName("ipv4")]
        public string IPv4 { get; set; } = string.Empty;
        
        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }
    }
}
