using MongoKafkaConsumerService.Kafka;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
using MongoDB.Driver;
using MongoKafkaConsumerService.User;

namespace MongoKafkaConsumerService.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaConsumerService> _logger;
        private IConsumer<Ignore, string> _consumer;
        private readonly IMongoCollection<UserProfile> _profiles;

        public KafkaConsumerService(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaConsumerService> logger, IMongoDatabase mongoDatabase)
        {
            _kafkaSettings = kafkaSettings.Value;
            _logger = logger;

            // Get the profiles collection from the injected IMongoDatabase
            _profiles = mongoDatabase.GetCollection<UserProfile>("profiles");

            var conf = new ConsumerConfig
            {
                GroupId = "my-group",
                BootstrapServers = _kafkaSettings.BootstrapServers,
            };

            _consumer = new ConsumerBuilder<Ignore, string>(conf).Build();

            _logger.LogInformation("Kafka consumer connected successfully to {BootstrapServers}.", _kafkaSettings.BootstrapServers);

            _consumer.Subscribe(new List<string> { "RegisterUser", "UserActivation", "UserDeactivation" });
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
                            var activationUserId = consumeResult.Message.Value;

                            // Update the activation status of the user profile
                            var activationProfile = _profiles.Find(p => p.UserId == activationUserId).FirstOrDefault();
                            if (activationProfile != null)
                            {
                                activationProfile.IsActive = true;
                                _profiles.ReplaceOne(p => p.UserId == activationUserId, activationProfile);
                                _logger.LogInformation($"Activated user profile for userId: {activationUserId}");
                            } else
                            {
                                _logger.LogInformation($"User profile not found for activation of userId: {activationUserId}");
                            }
                            break;

                        case "UserDeactivation":
                            var deactivationUserId = consumeResult.Message.Value;

                            // Update the activation status of the user profile
                            var deactivationProfile = _profiles.Find(p => p.UserId == deactivationUserId).FirstOrDefault();
                            if (deactivationProfile != null)
                            {
                                deactivationProfile.IsActive = false;
                                _profiles.ReplaceOne(p => p.UserId == deactivationUserId, deactivationProfile);
                                _logger.LogInformation($"Deactivated user profile for userId: {deactivationUserId}");
                            } else
                            {
                                _logger.LogInformation($"User profile not found for deactivation of userId: {deactivationUserId}");
                            }
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
