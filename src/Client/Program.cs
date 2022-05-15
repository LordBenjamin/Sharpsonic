using System;
using System.Net.Http;
using Auricular.Client;
using Auricular.Client.MediaPlayer;
using Auricular.Client.Security;
using Auricular.Client.Services;
using Blazored.LocalStorage;
using Howler.Blazor.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiConfigSection = builder.Configuration.GetSection("Api");

// Configure HTTP Client
builder.Services.AddSingleton<CookieHandler>();
builder.Services
    .AddHttpClient("WebAPI", client => client.BaseAddress = new Uri(apiConfigSection["BaseUrl"]))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddSingleton(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("WebAPI"));

// Configure Auth
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<CustomAuthStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>(sp => sp.GetService<CustomAuthStateProvider>()!);

// Configure Player
builder.Services.AddSingleton<IHowl, Howl>();
builder.Services.AddSingleton<IHowlGlobal, HowlGlobal>();
builder.Services.AddSingleton<PlayerState>();

// Configure API client services
builder.Services.AddSingleton<AlbumSongListService>();
builder.Services.AddSingleton<MediaRetrievalService>();
builder.Services.AddSingleton<BrowsingService>();
builder.Services.AddSingleton<AuthenticationService>();

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
