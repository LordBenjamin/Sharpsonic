using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Auricular.Client.Services;
using Auricular.DataTransfer.Security;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

namespace Auricular.Client.Security {
    public class CustomAuthStateProvider : AuthenticationStateProvider {
        private static readonly ClaimsPrincipal EmptyClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        private ClaimsPrincipal claimsPrincipal = EmptyClaimsPrincipal;

        public CustomAuthStateProvider(AuthenticationService authenticationService, ILocalStorageService localStorage) {
            ArgumentNullException.ThrowIfNull(authenticationService);
            ArgumentNullException.ThrowIfNull(localStorage);

            AuthenticationService = authenticationService;
            LocalStorage = localStorage;
        }

        public NetworkCredential Credential { get; private set; } = new NetworkCredential();

        public string? UserName => claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public AuthenticationService AuthenticationService { get; }
        public ILocalStorageService LocalStorage { get; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
            const string authStateKey = "authState";

            CustomAuthState? state = await LocalStorage.GetItemAsync<CustomAuthState>(authStateKey);

            if (state?.UserName is string userName) {
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, userName),
                    }, CookieAuthenticationDefaults.AuthenticationScheme));
            } else {
                if (Credential.UserName?.Length > 0) {
                    LoginResponse? response = await AuthenticationService.TryLogin(Credential.UserName, Credential.Password);

                    if (response != null) {
                        claimsPrincipal = new ClaimsPrincipal(
                            new ClaimsIdentity(new Claim[] {
                                new Claim(ClaimTypes.NameIdentifier, response.UserName),
                            }, CookieAuthenticationDefaults.AuthenticationScheme));

                        await LocalStorage.SetItemAsync(authStateKey, new CustomAuthState {
                            UserName = response.UserName,
                        });
                    } else {
                        claimsPrincipal = EmptyClaimsPrincipal;
                    }
                } else {
                    claimsPrincipal = EmptyClaimsPrincipal;
                }
            }

            return new AuthenticationState(claimsPrincipal);
        }

        public void SetCredentials(NetworkCredential credential) {
            ArgumentNullException.ThrowIfNull(credential);

            Credential = credential;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
