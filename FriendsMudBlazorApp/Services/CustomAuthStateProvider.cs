using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FriendsMudBlazorApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private bool _authenticated = false;

        public void MarkUserAsAuthenticated()
        {
            _authenticated = true;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            _authenticated = false;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_authenticated)
            {
                var identity = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, "username") }, "apiauth_type");
                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));
            } else
            {
                var identity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));
            }
        }
    }

}
