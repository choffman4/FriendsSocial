using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SignalR.Data;
using Microsoft.AspNetCore.ResponseCompression;
using SignalR.Hubs;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Services;
//using SignalR.Services;

namespace SignalR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Retrieve the gRPC service configuration from appsettings.json
            var grpcConfig = builder.Configuration.GetSection("GrpcService");
            string grpcHost = grpcConfig["Host"];
            string grpcPort = grpcConfig["Port"];
            var grpcAddress = $"http://{grpcHost}:{grpcPort}";

            var channel = GrpcChannel.ForAddress(grpcAddress);
            var grpcClient = new GrpcMongoMessagingService.MongoMessagingService.MongoMessagingServiceClient(channel);
            builder.Services.AddSingleton(x =>
            {
                var logger = x.GetRequiredService<ILogger<MessagingServiceClientWrapper>>();
                return new Services.MessagingServiceClientWrapper(grpcClient, logger);
            });


            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();

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
            //endpoints for mapping the Blazor hub and the client-side fallback
            app.MapHub<ChatHub>("/chathub");


            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}