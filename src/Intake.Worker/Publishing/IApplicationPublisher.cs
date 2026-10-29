using Lab.Intake.Contracts;

namespace Intake.Worker.Publishing;

public interface IApplicationPublisher
{
    Task PublishSubmitted(LeaseApplicationSubmitted application, CancellationToken cancellationToken);

    Task PublishEnriched(LeaseApplicationEnriched enrichment, CancellationToken cancellationToken);
}
