using System;
using System.Net.Http;
using Auricular.Client;
using Auricular.Client.MediaPlayer;
using Howler.Blazor.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<IHowl, Howl>();
builder.Services.AddSingleton<IHowlGlobal, HowlGlobal>();
builder.Services.AddSingleton<PlayerState>();


await builder.Build().RunAsync();
