using MongoKafkaConsumerService.Kafka;
using MongoKafkaConsumerService.Services;
using MongoDB.Driver;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery;

namespace MongoKafkaConsumerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure the services here
                    services.Configure<KafkaSettings>(hostContext.Configuration.GetSection("Kafka"));
                    services.AddHostedService<KafkaConsumerService>();

                    // Retrieve the MongoDB connection string from app settings
                    var mongoConnectionString = hostContext.Configuration.GetConnectionString("MongoDb");
                    var mongoClient = new MongoClient(mongoConnectionString);
                    var mongoDatabase = mongoClient.GetDatabase("profileDatabase");

                    // Register the IMongoDatabase as a singleton service
                    services.AddSingleton(mongoDatabase);

                    // Add Eureka Discovery Client
                    services.AddDiscoveryClient(hostContext.Configuration);
                });

            var app = builder.Build();

            // Application started event
            app.Services.GetService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
            {
                // Start Eureka client
                app.Services.GetService<IDiscoveryClient>();
            });

            app.Run();
        }
    }
}
