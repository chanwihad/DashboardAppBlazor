using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;


namespace DashboardApp.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {

        private readonly IJSRuntime _jsRuntime;

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = null;

            try
            {
                token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "Token");
            }
            catch (InvalidOperationException)
            {
                // Terjadi pada fase prerendering
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(ParseClaimsFromToken(token), "Bearer");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private IEnumerable<Claim> ParseClaimsFromToken(string token)
{
    var claims = new List<Claim>();

    // Ambil payload dari token JWT
    var payload = token.Split('.')[1];

    // Tambahkan padding jika perlu
    payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

    // Dekode Base64
    var jsonBytes = Convert.FromBase64String(payload);

    // Parse JSON menjadi dictionary
    var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

    foreach (var kvp in keyValuePairs)
    {
        claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
    }

    return claims;
}


        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}