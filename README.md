# lab-intake

Intake worker of the equipment-leasing lab. `POST /intake/applications` produces two Avro events to Kafka through the MassTransit rider:

| Topic (configuration key) | Default | Record |
|---|---|---|
| `Kafka:Topics:Applications` | `lease-applications-v1` | `Lab.Intake.Contracts.LeaseApplicationSubmitted` |
| `Kafka:Topics:Enrichment` | `lease-application-enrichment-v1` | `Lab.Intake.Contracts.LeaseApplicationEnriched` |

Schemas are in `src/Intake.Contracts/Schemas`. `Kafka:Enabled=false` replaces the producers with a logging publisher.

## Build

Docker only:

```bash
docker build --target test .
docker build --target runtime -t lab-intake .
```

## License

MIT, see [LICENSE](LICENSE).
