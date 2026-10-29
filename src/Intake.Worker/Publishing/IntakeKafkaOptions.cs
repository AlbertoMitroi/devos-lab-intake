namespace Intake.Worker.Publishing;

public sealed class IntakeKafkaOptions
{
    public const string SectionName = "Kafka";

    public bool Enabled { get; set; } = true;

    public string BootstrapServers { get; set; } = "";

    public string SchemaRegistryUrl { get; set; } = "";

    public IntakeTopics Topics { get; set; } = new();
}

public sealed class IntakeTopics
{
    public string Applications { get; set; } = "lease-applications-v1";

    public string Enrichment { get; set; } = "lease-application-enrichment-v1";
}
