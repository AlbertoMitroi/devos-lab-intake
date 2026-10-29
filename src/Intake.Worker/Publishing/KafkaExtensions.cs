using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Lab.Intake.Contracts;
using MassTransit;

namespace Intake.Worker.Publishing;

public static class KafkaExtensions
{
    /// <summary>Kafka rider with Avro serialization; topic names come from <c>Kafka:Topics</c>.</summary>
    public static IServiceCollection AddIntakeKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(IntakeKafkaOptions.SectionName).Get<IntakeKafkaOptions>() ?? new IntakeKafkaOptions();
        if (!options.Enabled)
        {
            services.AddSingleton<IApplicationPublisher, LoggingApplicationPublisher>();
            return services;
        }

        services.AddSingleton<ISchemaRegistryClient>(_ =>
            new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = options.SchemaRegistryUrl }));

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();
            x.AddRider(rider =>
            {
                rider.AddProducer<string, LeaseApplicationSubmitted>(options.Topics.Applications, (context, producer) =>
                    producer.SetValueSerializer(new AvroSerializer<LeaseApplicationSubmitted>(context.GetRequiredService<ISchemaRegistryClient>())));

                rider.AddProducer<string, LeaseApplicationEnriched>(options.Topics.Enrichment, (context, producer) =>
                    producer.SetValueSerializer(new AvroSerializer<LeaseApplicationEnriched>(context.GetRequiredService<ISchemaRegistryClient>())));

                rider.UsingKafka((_, kafka) => kafka.Host(options.BootstrapServers));
            });
        });

        services.AddScoped<IApplicationPublisher, KafkaApplicationPublisher>();
        return services;
    }
}
