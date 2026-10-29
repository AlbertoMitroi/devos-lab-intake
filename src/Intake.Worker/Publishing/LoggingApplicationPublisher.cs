using Lab.Intake.Contracts;

namespace Intake.Worker.Publishing;

/// <summary>Used when <c>Kafka:Enabled</c> is false: the events are only logged.</summary>
public sealed class LoggingApplicationPublisher(ILogger<LoggingApplicationPublisher> logger) : IApplicationPublisher
{
    public Task PublishSubmitted(LeaseApplicationSubmitted application, CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka disabled, application {ApplicationId} not published", application.ApplicationId);
        return Task.CompletedTask;
    }

    public Task PublishEnriched(LeaseApplicationEnriched enrichment, CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka disabled, enrichment {ApplicationId} not published", enrichment.ApplicationId);
        return Task.CompletedTask;
    }
}
