using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using GrpcSqlUserService.Kafka;
using GrpcSqlUserService.Services;

namespace GrpcSqlUserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var kafkaBootstrapServers = builder.Configuration.GetSection("Kafka")["BootstrapServers"];
            builder.Services.AddGrpc();
            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

            var app = builder.Build();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            CreateKafkaTopic(kafkaBootstrapServers, logger).Wait();

            app.MapGrpcService<UserGrpcService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

        private static async Task CreateKafkaTopic(string bootstrapServers, ILogger logger)
        {
            var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

            using var adminClient = new AdminClientBuilder(config).Build();
            try
            {
                logger.LogInformation("Trying to create Kafka topic...");

                await adminClient.CreateTopicsAsync(new List<TopicSpecification>
                {
                    new TopicSpecification { Name = "RegisterUser", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "UserActivation", NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = "UserDeactivation", NumPartitions = 1, ReplicationFactor = 1 }
                });

                logger.LogInformation("Kafka topic 'RegisterUser' created successfully. Kafka topic 'UserActivation' created successfully.");
            } catch (CreateTopicsException e)
            {
                if (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    logger.LogWarning("Topic already exists. Skipping topic creation.");
                } else
                {
                    logger.LogError($"An error occurred creating the topic: {e.Results[0].Error.Reason}");
                }
            } catch (Exception ex)
            {
                logger.LogError($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
