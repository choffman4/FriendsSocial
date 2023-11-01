using MongoKafkaConsumerService.Kafka;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
using MongoDB.Driver;
using MongoKafkaConsumerService.User;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using System.Numerics;


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
                            var userMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<UserProfile>(consumeResult.Message.Value);

                            // Check if a profile already exists for this userId
                            var existingProfile = _profiles.Find(p => p.UserId == userMessage.UserId).FirstOrDefault();
                            if (existingProfile == null)
                            {
                                bool isAValidUsername = false;
                                string generatedUsername = "";
                                while(isAValidUsername == false)
                                {
                                    // Generate a unique username for the user
                                    generatedUsername = userMessage.FirstName + GenerateUniquePart(userMessage.FirstName);
                                    if(await UsernameAlreadyExists(generatedUsername) == false)
                                    {
                                        isAValidUsername = true;
                                    }
                                }
                                

                                // Create a new user profile using the deserialized data
                                var userProfile = new UserProfile(userMessage.UserId)
                                {
                                    Username = generatedUsername,
                                    FirstName = userMessage.FirstName,
                                    LastName = userMessage.LastName,
                                    DateOfBirth = userMessage.DateOfBirth,
                                    Gender = userMessage.Gender
                                };

                                _profiles.InsertOne(userProfile);
                                _logger.LogInformation($"Created a user profile for userId: {userMessage.UserId}");
                            } else
                            {
                                _logger.LogInformation($"Profile already exists for userId: {userMessage.UserId}");
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

        public async Task<bool> UsernameAlreadyExists(string username)
        {
            var existingProfile = _profiles.Find(p => p.Username == username).FirstOrDefault();
            //is the username in use?
            if(existingProfile != null)
            {
                return true;
            } else //username not in use
            { return false; }
        }

        private string GenerateUniquePart(string seed)
        {
            // Using a combination of timestamp and a part of UUID
            string timestamp = DateTime.UtcNow.Ticks.ToString();
            string uuidPart = Guid.NewGuid().ToString().Substring(0, 4); // taking only the first 4 characters

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(seed + timestamp + uuidPart));

                // Convert byte array to BigInteger and then to string
                BigInteger intRepresentation = new BigInteger(bytes.Reverse().ToArray());
                return intRepresentation.ToString().Substring(0, 8); // take first 8 characters of the numeric hash
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
