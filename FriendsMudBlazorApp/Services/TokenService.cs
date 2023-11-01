using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;

namespace FriendsMudBlazorApp.Services
{
    public class TokenService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "jwtToken";
        private const string UserGuidKey = "userGuid";
        private const string ExpiryKey = "expiryTime";

        private readonly AuthenticationStateProvider _authStateProvider;

        public TokenService(AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _authStateProvider = authStateProvider;
        }


        public async Task SetTokenAsync(string token)
        {
            var expiryTime = DateTime.UtcNow.AddHours(1).ToString("o"); // ISO 8601 format

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserGuidKey, ExtractGuidFromToken(token));
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ExpiryKey, expiryTime);
        }

        public async Task<string> GetTokenAsync()
        {
            await ClearExpiredDataAsync();

            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
        }

        public async Task<string> GetUserGuidAsync()
        {
            await ClearExpiredDataAsync();

            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserGuidKey);
        }

        public async Task ClearExpiredDataAsync()
        {
            var storedExpiryTime = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ExpiryKey);
            if (!string.IsNullOrEmpty(storedExpiryTime))
            {
                var expiryTime = DateTime.Parse(storedExpiryTime);
                if (DateTime.UtcNow > expiryTime)
                {
                    // The data has expired, so we clear it.
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserGuidKey);
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ExpiryKey);

                    // We also need to clear the authentication state.
                    if (_authStateProvider is CustomAuthStateProvider customProvider)
                    {
                        customProvider.MarkUserAsLoggedOut();
                    }

                }
            }
        }

        private string ExtractGuidFromToken(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);

            // Assuming the GUID is stored in a claim. Adjust accordingly.
            var guidClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "nameid");

            return guidClaim?.Value;
        }
    }

}
