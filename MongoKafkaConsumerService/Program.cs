using MongoKafkaConsumerService.Kafka;
using MongoKafkaConsumerService.Services;
using MongoDB.Driver;

namespace MongoKafkaConsumerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new HostBuilder()
                            .ConfigureAppConfiguration((hostingContext, config) =>
                            {
                                config.AddJsonFile("appsettings.json", optional: true);
                            })
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
                            });

            builder.Build().Run();
        }
    }
}