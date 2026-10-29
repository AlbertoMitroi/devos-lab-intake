using Lab.Intake.Contracts;
using MassTransit;

namespace Intake.Worker.Publishing;

/// <summary>Produces intake events to Kafka through the MassTransit rider; the key is the application id.</summary>
public sealed class KafkaApplicationPublisher(
    ITopicProducer<string, LeaseApplicationSubmitted> submittedProducer,
    ITopicProducer<string, LeaseApplicationEnriched> enrichedProducer) : IApplicationPublisher
{
    public Task PublishSubmitted(LeaseApplicationSubmitted application, CancellationToken cancellationToken) =>
        submittedProducer.Produce(application.ApplicationId, application, cancellationToken);

    public Task PublishEnriched(LeaseApplicationEnriched enrichment, CancellationToken cancellationToken) =>
        enrichedProducer.Produce(enrichment.ApplicationId, enrichment, cancellationToken);
}
