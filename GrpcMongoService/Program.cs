using GrpcMongoService.Kafka;
using GrpcMongoService.Services;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GrpcMongoService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure the services here
            builder.Services.AddGrpc();
            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
            builder.Services.AddHostedService<KafkaConsumerService>();
            // other service configurations...

            builder.Services.AddSingleton<MongoClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var mongoConnectionString = configuration.GetConnectionString("MongoDb");
                return new MongoClient(mongoConnectionString);
            });

            builder.Services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<MongoClient>();
                return client.GetDatabase("mongodb");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<ProfileGrpcService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

    }
}