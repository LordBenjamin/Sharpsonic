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

var apiConfigSection = builder.Configuration.GetSection("Api");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(apiConfigSection["BaseUrl"]) });

builder.Services.AddSingleton<IHowl, Howl>();
builder.Services.AddSingleton<IHowlGlobal, HowlGlobal>();
builder.Services.AddSingleton<PlayerState>();

builder.Services.AddSingleton<AlbumSongListService>();
builder.Services.AddSingleton<MediaRetrievalService>();
builder.Services.AddSingleton<BrowsingService>();

await builder.Build().RunAsync();
