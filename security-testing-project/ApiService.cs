using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; // Assuming Newtonsoft.Json is available for JSON serialization/deserialization

namespace security_testing_project
{
    public static class ApiService
    {
        private static readonly HttpClient _httpClient;
        public static string? JwtToken { get; private set; }
        public static string? Username { get; private set; }
        public static string? Role { get; private set; }

        static ApiService()
        {
            _httpClient = new HttpClient();
            // Base address for the API - adjust if your API runs on a different port or URL
            _httpClient.BaseAddress = new Uri("https://localhost:7290/"); // Example API URL
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static bool IsLoggedIn => !string.IsNullOrEmpty(JwtToken);

        public static void SetJwtToken(string token, string username, string role)
        {
            JwtToken = token;
            Username = username;
            Role = role;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
        }

        public static void ClearSession()
        {
            JwtToken = null;
            Username = null;
            Role = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public static async Task<(bool Success, string Message)> LoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new { username = username, password = password };
                var jsonContent = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Auth/login", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                    
                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        SetJwtToken(loginResponse.Token, loginResponse.Username ?? username, loginResponse.Role ?? "Player");
                        return (true, "Login successful.");
                    }
                    return (false, "Login failed: Invalid response from API.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Attempt to deserialize API error message if available
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(errorContent);
                        return (false, $"Login failed: {errorResponse?.Message ?? response.ReasonPhrase}");
                    }
                    catch
                    {
                        return (false, $"Login failed: {response.ReasonPhrase} - {errorContent}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Login failed due to network error: {ex.Message}. Is the API running?");
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred during login: {ex.Message}");
            }
        }
        
        public static async Task<(bool Success, string Content, string Message)> GetAuthenticatedAsync(string requestUri)
        {
            try
            {
                if (!IsLoggedIn)
                {
                    return (false, string.Empty, "Not logged in.");
                }

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return (true, content, "Request successful.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(errorContent);
                        return (false, string.Empty, $"Request failed: {errorResponse?.Message ?? response.ReasonPhrase}");
                    }
                    catch
                    {
                        return (false, string.Empty, $"Request failed: {response.ReasonPhrase} - {errorContent}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, string.Empty, $"Request failed due to network error: {ex.Message}. Is the API running?");
            }
            catch (Exception ex)
            {
                return (false, string.Empty, $"An unexpected error occurred during request: {ex.Message}");
            }
        }
    }

    // Helper classes for JSON deserialization
    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }

    public class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}
