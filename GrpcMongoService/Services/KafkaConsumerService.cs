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
            };

            _consumer = new ConsumerBuilder<Ignore, string>(conf).Build();

            _logger.LogInformation("Kafka consumer connected successfully to {BootstrapServers}.", _kafkaSettings.BootstrapServers);

            _consumer.Subscribe(new List<string> { "RegisterUser", "UserActivation" });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    var currentTopic = consumeResult.Topic;

                    _logger.LogInformation($"Received message at {consumeResult.TopicPartitionOffset}: {consumeResult.Value}");

                    switch (currentTopic)
                    {
                        case "RegisterUser":
                            var userId = consumeResult.Message.Value;

                            // Check if a profile already exists for this userId
                            var existingProfile = _profiles.Find(p => p.UserId == userId).FirstOrDefault();
                            if (existingProfile == null)
                            {
                                // Create a new user profile
                                var userProfile = new UserProfile(userId);

                                _profiles.InsertOne(userProfile);
                                _logger.LogInformation($"Created a user profile for userId: {userId}");
                            } else
                            {
                                _logger.LogInformation($"Profile already exists for userId: {userId}");
                            }
                            break;

                        // Add more cases for handling other topics
                        case "UserActivation":
                            // Handle messages from "AnotherTopic"
                            break;

                        // Add additional cases as needed

                        default:
                            _logger.LogInformation($"Received unknown topic {currentTopic} at {consumeResult.TopicPartitionOffset}: {consumeResult.Value}");
                            break;
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
