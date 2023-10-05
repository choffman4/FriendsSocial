using GrpcMongoService.Kafka;
using GrpcMongoService.Services;
using Microsoft.Extensions.Configuration;

namespace GrpcMongoService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Configure the services here
            builder.Services.AddGrpc();
            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
            builder.Services.AddHostedService<KafkaConsumerService>();
            // other service configurations...

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<ProfileGrpcService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

    }
}