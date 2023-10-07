using GrpcMongoService.Kafka;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
using MongoDB.Driver;
using GrpcMongoService.User;

namespace GrpcMongoService.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaConsumerService> _logger;
        private IConsumer<Ignore, string> _consumer;
        private readonly IMongoCollection<UserProfile> _profiles;

        public KafkaConsumerService(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaConsumerService> logger, MongoClient mongoClient)
        {
            _kafkaSettings = kafkaSettings.Value;
            _logger = logger;

            // Connect to the profileDatabase and get the profiles collection
            var profileDatabase = mongoClient.GetDatabase("profileDatabase");
            _profiles = profileDatabase.GetCollection<UserProfile>("profiles");

            // If you want to connect to the profilePosts database and get the posts collection:
            // var postsDatabase = mongoClient.GetDatabase("profilePosts");
            // _posts = postsDatabase.GetCollection<Post>("posts");

            var conf = new ConsumerConfig
            {
                GroupId = "my-group",
                BootstrapServers = _kafkaSettings.BootstrapServers,
                // ... other necessary config values
            };

            _consumer = new ConsumerBuilder<Ignore, string>(conf).Build();
            _consumer.Subscribe("RegisterUser");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult.Topic == "RegisterUser")
                    {
                        var userId = consumeResult.Message.Value;

                        // Check if a profile already exists for this userId
                        var existingProfile = _profiles.Find(p => p.UserId == userId).FirstOrDefault(); // Corrected this line
                        if (existingProfile == null)
                        {
                            // Create a new user profile
                            var userProfile = new UserProfile { UserId = userId };

                            _profiles.InsertOne(userProfile); // Corrected this line
                            _logger.LogInformation($"Created a user profile for userId: {userId}");
                        } else
                        {
                            _logger.LogInformation($"Profile already exists for userId: {userId}");
                        }
                    } else
                    {
                        _logger.LogInformation($"Received unknown topic {consumeResult.Topic} at {consumeResult.TopicPartitionOffset}: {consumeResult.Value}");
                    }
                }
            } catch (OperationCanceledException)
            {
                // No need to do anything special here.
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _consumer.Close();
            return base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
