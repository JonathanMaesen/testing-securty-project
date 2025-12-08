using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using API.Models;
using System.Threading;
using Newtonsoft.Json;

namespace security_testing_project
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string? JwtToken { get; set; }
        public string? Username { get; private set; }
        public string? Role { get; private set; }

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Base address for the API - adjust if your API runs on a different port or URL
            _httpClient.BaseAddress = new Uri("http://localhost:5263"); // Example API URL
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public bool IsLoggedIn => !string.IsNullOrEmpty(JwtToken);

        private void SetJwtToken(string token, string username, string role)
        {
            JwtToken = token;
            Username = username;
            Role = role;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
        }

        public void ClearSession()
        {
            JwtToken = null;
            Username = null;
            Role = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<(bool Success, string Message)> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                var loginRequest = new LoginRequest { Username = username, Password = password };
                var jsonContent = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Auth/login", jsonContent, cancellationToken);

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
                    return (false, $"Login failed: {response.ReasonPhrase} - {errorContent}");
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

        public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, string role = "Player", CancellationToken cancellationToken = default)
        {
            try
            {
                var registerRequest = new RegisterRequest { Username = username, Password = password, Role = role };
                var jsonContent = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Auth/register", jsonContent, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Attempt to deserialize the response, regardless of status code, as the body may contain useful info.
                var registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    // If there's a message in the deserialized response, use it, otherwise return the raw content.
                    return (false, registerResponse?.Message ?? responseContent);
                }

                if (registerResponse is { Success: true })
                {
                    return (true, registerResponse.Message ?? "Registration successful.");
                }
                return (false, registerResponse?.Message ?? "Registration failed: Invalid response from API.");
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Registration failed due to network error: {ex.Message}. Is the API running?");
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred during registration: {ex.Message}");
            }
        }

        public async Task<(bool Success, List<UserDto>? Users, string Message)> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            // Ensure the user is logged in, as this endpoint is likely protected.
            if (!IsLoggedIn)
            {
                return (false, null, "You must be logged in to view users.");
            }

            try
            {
                var response = await _httpClient.GetAsync("api/users", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<UserDto>>(responseContent);
                    return (true, users, "Successfully retrieved users.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, null, $"Failed to get users: {response.ReasonPhrase} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, null, $"Network error: {ex.Message}. Is the API running?");
            }
            catch (Exception ex)
            {
                return (false, null, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}