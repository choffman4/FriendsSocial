using GrpcServiceTemp.Kafka;
using GrpcServiceTemp.Services;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GrpcServiceTemp
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
            //// other service configurations...

            // Retrieve the MongoDB connection string from app settings
            var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
            var mongoClient = new MongoClient(mongoConnectionString);
            var mongoDatabase = mongoClient.GetDatabase("profileDatabase");

            // Register the IMongoDatabase as a singleton service
            builder.Services.AddSingleton(mongoDatabase);

            var app = builder.Build();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<ProfileGrpcService>();
            logger.LogInformation("gRPC service started successfully.");
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}