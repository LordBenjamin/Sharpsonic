using System;
using System.Net.Http;
using Auricular.Client;
using Auricular.Client.MediaPlayer;
using Auricular.Client.Services;
using Howler.Blazor.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });

builder.Services.AddSingleton<IHowl, Howl>();
builder.Services.AddSingleton<IHowlGlobal, HowlGlobal>();
builder.Services.AddSingleton<PlayerState>();

builder.Services.AddScoped<AlbumSongListService>();
builder.Services.AddScoped<MediaRetrievalService>();
builder.Services.AddScoped<BrowsingService>();

await builder.Build().RunAsync();
