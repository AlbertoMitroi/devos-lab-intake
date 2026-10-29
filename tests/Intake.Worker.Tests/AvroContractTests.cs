using Avro.IO;
using Avro.Specific;
using FluentAssertions;
using Lab.Intake.Contracts;

namespace Intake.Worker.Tests;

public class AvroContractTests
{
    [Fact]
    public void Submitted_application_round_trips_through_avro()
    {
        var original = new LeaseApplicationSubmitted
        {
            ApplicationId = "a-1",
            ApplicantName = "Northwind Haulage",
            CompanyNumber = "RO123",
            AssetType = "forklift",
            AssetValue = 36_000,
            TermMonths = 36,
            SubmittedAtUnixMs = 1_700_000_000_000,
        };

        using var stream = new MemoryStream();
        new SpecificDatumWriter<LeaseApplicationSubmitted>(LeaseApplicationSubmitted._SCHEMA).Write(original, new BinaryEncoder(stream));
        stream.Position = 0;
        var copy = new SpecificDatumReader<LeaseApplicationSubmitted>(LeaseApplicationSubmitted._SCHEMA, LeaseApplicationSubmitted._SCHEMA)
            .Read(new LeaseApplicationSubmitted(), new BinaryDecoder(stream));

        copy.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Schema_file_matches_the_record_schema()
    {
        var file = Avro.Schema.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Schemas", "LeaseApplicationEnriched.avsc")));

        file.Should().Be(LeaseApplicationEnriched._SCHEMA);
    }
}
