using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DashboardApp.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;  
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Authorization;
using DashboardApp.Services;

namespace DashboardApp.Services
{

    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _clientId;  
        private readonly string _secretKey;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly PermissionHelper _permissionHelper;

        public AuthApiClient(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime, AuthenticationStateProvider authenticationStateProvider, PermissionHelper permissionHelper)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _secretKey = _configuration["ApiSettings:SecretKey"];
            _jsRuntime = jsRuntime;
            _authenticationStateProvider = authenticationStateProvider;
            _permissionHelper = permissionHelper;
        }

        public async Task InitializeClientIdAsync()
        {
            var isLogin = await _permissionHelper.CheckLogin();

            if (isLogin)
            {
                _clientId = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "ClientId");
            }
            else
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }
        }

        public async Task<bool> RegisterAsync(string username, string fullName, string email, string password)
        {
            var registerModel = new
            {
                Username = username,
                FullName = fullName,
                Email = email,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(registerModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/register", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginModel = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(loginModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
                if (result != null)
                {
                    // _httpContextAccessor.HttpContext.Session.SetString("Token", result.Token);
                    // _httpContextAccessor.HttpContext.Session.SetString("Username", result.Username);
                    // _httpContextAccessor.HttpContext.Session.SetString("ClientId", result.ClientId);
                    // _httpContextAccessor.HttpContext.Session.SetString("UserMenus", JsonSerializer.Serialize(result.Menus));
                    // _httpContextAccessor.HttpContext.Session.SetString("CanCreate", result.Permissions.CanCreate.ToString().ToLower());
                    // _httpContextAccessor.HttpContext.Session.SetString("CanView", result.Permissions.CanView.ToString().ToLower());
                    // _httpContextAccessor.HttpContext.Session.SetString("CanUpdate", result.Permissions.CanUpdate.ToString().ToLower());
                    // _httpContextAccessor.HttpContext.Session.SetString("CanDelete", result.Permissions.CanDelete.ToString().ToLower());

                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "Token", result.Token);
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "Username", result.Username);
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "ClientId", result.ClientId);
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "UserMenus", JsonSerializer.Serialize(result.Menus));
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "CanCreate", result.Permissions.CanCreate.ToString().ToLower());
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "CanView", result.Permissions.CanView.ToString().ToLower());
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "CanUpdate", result.Permissions.CanUpdate.ToString().ToLower());
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "CanDelete", result.Permissions.CanDelete.ToString().ToLower());

                    var authStateProvider = (CustomAuthenticationStateProvider)_authenticationStateProvider;
                    authStateProvider.NotifyAuthenticationStateChanged();

                    return true;
                }
            }

            return false;
        }

        public async Task<HttpResponseMessage> ChangePasswordAsync(ChangePasswordModel model)
        {
            await InitializeClientIdAsync(); 
            var body = JsonSerializer.Serialize(model);
            AddSecurityHeaders("POST", "api/auth/change-password", body);
            var response = await _httpClient.PostAsJsonAsync("/api/auth/change-password", model);
            return response;
        }

        public async Task<HttpResponseMessage> SendVerificationCode(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/send-verif", email);

            return response;
        }
        
        public async Task<HttpResponseMessage> VerifyCode(VerificationRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/verify-code", model);

            return response;
        }

        public async Task<HttpResponseMessage> ResetPassword(NewPassword model)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/reset-password", model);

            return response;
        }

        public async Task<bool> LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.clear");
            var authStateProvider = (CustomAuthenticationStateProvider)_authenticationStateProvider;
            authStateProvider.NotifyAuthenticationStateChanged();
            return true;
        }


        private string GenerateSignature(string method, string rawUrl, string clientId, string timeStamp, string body)
        {
            string strToSign = $"{method}:{rawUrl}:{clientId}:{timeStamp}:{body}";
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(strToSign));
                return Convert.ToBase64String(hash);
            }
        }

        private void AddSecurityHeaders(string method, string rawUrl, string body)
        {
            var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var signature = GenerateSignature(method, rawUrl, _clientId, timeStamp, body);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Client-ID", _clientId);
            _httpClient.DefaultRequestHeaders.Add("X-Time-Stamp", timeStamp);
            _httpClient.DefaultRequestHeaders.Add("X-Signature", signature);
        }

    }
}