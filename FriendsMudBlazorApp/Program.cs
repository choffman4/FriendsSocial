global using Microsoft.AspNetCore.Components.Authorization;
global using Blazored.LocalStorage;
using FriendsMudBlazorApp.Data;
using FriendsMudBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using GrpcMongoMessagingService;
using FriendsMudBlazorApp.Hubs;

namespace FriendsMudBlazorApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddScoped<HttpClient>();
            builder.Services.AddMudServices();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSignalR();

            builder.Services.AddGrpcClient<MongoMessagingService.MongoMessagingServiceClient>(o =>
            {
                o.Address = new Uri("http://localhost:8008"); // Replace with your gRPC server's address
            });

            builder.Services.AddSingleton<MessagingServiceClientWrapper>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapHub<MessageHub>("/messagehub");

            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}

