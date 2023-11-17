global using Microsoft.AspNetCore.Components.Authorization;
global using Blazored.LocalStorage;
using FriendsMudBlazorApp.Services;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using GrpcMongoMessagingService;
using FriendsMudBlazorApp.Hubs;
using Grpc.Net.Client;
using Microsoft.AspNetCore.ResponseCompression;


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


            // Retrieve the gRPC service configuration from appsettings.json
            var grpcConfig = builder.Configuration.GetSection("GrpcService");
            string grpcHost = grpcConfig["Host"];
            string grpcPort = grpcConfig["Port"];
            var grpcAddress = $"http://{grpcHost}:{grpcPort}";

            var channel = GrpcChannel.ForAddress(grpcAddress);
            var grpcClient = new MongoMessagingService.MongoMessagingServiceClient(channel);
            builder.Services.AddSingleton(x =>
            {
                var logger = x.GetRequiredService<ILogger<MessagingServiceClientWrapper>>();
                return new Services.MessagingServiceClientWrapper(grpcClient, logger);
            });


            //SignalR
            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                      new[] { "application/octet-stream" });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //SignalR - Compression Middleware
            app.UseResponseCompression();

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

