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
            // Basisadres voor de API - pas aan als de API op een andere poort of URL draait
            _httpClient.BaseAddress = new Uri("http://localhost:5263"); // Voorbeeld API URL
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
                        return (true, "Login succesvol.");
                    }
                    return (false, "Login mislukt: Ongeldig antwoord van de API.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"Login mislukt: {response.ReasonPhrase} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Login mislukt door netwerkfout: {ex.Message}. Draait de API wel?");
            }
            catch (Exception ex)
            {
                return (false, $"Een onverwachte fout is opgetreden tijdens het inloggen: {ex.Message}");
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

                // Probeer de response te deserialiseren, ongeacht de statuscode, omdat de body nuttige info kan bevatten.
                var registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    // Als er een bericht is in de gedeserialiseerde response, gebruik dat, anders de rauwe content.
                    return (false, registerResponse?.Message ?? responseContent);
                }

                if (registerResponse is { Success: true })
                {
                    return (true, registerResponse.Message ?? "Registratie succesvol.");
                }
                return (false, registerResponse?.Message ?? "Registratie mislukt: Ongeldig antwoord van de API.");
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Registratie mislukt door netwerkfout: {ex.Message}. Draait de API wel?");
            }
            catch (Exception ex)
            {
                return (false, $"Een onverwachte fout is opgetreden tijdens het registreren: {ex.Message}");
            }
        }

        public async Task<(bool Success, List<UserDto>? Users, string Message)> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            // Zorg ervoor dat de gebruiker is ingelogd, aangezien dit endpoint waarschijnlijk is beveiligd.
            if (!IsLoggedIn)
            {
                return (false, null, "U moet ingelogd zijn om gebruikers te zien.");
            }

            try
            {
                var response = await _httpClient.GetAsync("api/users", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<UserDto>>(responseContent);
                    return (true, users, "Gebruikers succesvol opgehaald.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, null, $"Ophalen van gebruikers mislukt: {response.ReasonPhrase} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, null, $"Netwerkfout: {ex.Message}. Draait de API wel?");
            }
            catch (Exception ex)
            {
                return (false, null, $"Een onverwachte fout is opgetreden: {ex.Message}");
            }
        }

        public async Task<string?> GetKeyShareAsync(string roomId)
        {
            if (!IsLoggedIn) return null;

            try
            {
                var response = await _httpClient.GetAsync($"api/keys/keyshare/{roomId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic? result = JsonConvert.DeserializeObject(content);
                    return result?.keyshare;
                }
                return null; 
            }
            catch
            {
                return null;
            }
        }
    }
}