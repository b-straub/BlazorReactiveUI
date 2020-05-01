using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorReactiveUI.Client.ViewModels;
using BlazorReactiveUI.Client.Reactive;

namespace BlazorReactiveUI.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped(sp => new IndexViewModel());
            builder.Services.AddScoped(typeof(ReactiveViewModel));
            builder.Services.AddScoped<IDataService>(sp => new DataService());
            await builder.Build().RunAsync();
        }
    }
}
