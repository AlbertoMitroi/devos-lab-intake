using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Intake.Worker.Endpoints;
using Intake.Worker.Publishing;
using Lab.Intake.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Intake.Worker.Tests;

public class ApplicationEndpointTests
{
    private sealed class RecordingPublisher : IApplicationPublisher
    {
        public List<object> Published { get; } = [];

        public Task PublishSubmitted(LeaseApplicationSubmitted application, CancellationToken cancellationToken)
        {
            Published.Add(application);
            return Task.CompletedTask;
        }

        public Task PublishEnriched(LeaseApplicationEnriched enrichment, CancellationToken cancellationToken)
        {
            Published.Add(enrichment);
            return Task.CompletedTask;
        }
    }

    private static (HttpClient Client, RecordingPublisher Publisher) Start()
    {
        var publisher = new RecordingPublisher();
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IApplicationPublisher>();
                s.AddSingleton<IApplicationPublisher>(publisher);
            });
        });
        return (factory.CreateClient(), publisher);
    }

    [Fact]
    public async Task Submitting_an_application_publishes_the_application_then_its_enrichment()
    {
        var (client, publisher) = Start();

        var response = await client.PostAsJsonAsync("/intake/applications", new SubmitApplicationRequest("Northwind Haulage", "RO123", "forklift", 36_000m, 36));

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var id = (await response.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;
        publisher.Published.Should().HaveCount(2);
        publisher.Published[0].Should().BeOfType<LeaseApplicationSubmitted>().Which.ApplicationId.Should().Be(id);
        publisher.Published[1].Should().BeOfType<LeaseApplicationEnriched>().Which.Should().BeEquivalentTo(Enrichment.For(id, "RO123"));
    }

    [Fact]
    public async Task Invalid_application_is_rejected_and_nothing_is_published()
    {
        var (client, publisher) = Start();

        var response = await client.PostAsJsonAsync("/intake/applications", new SubmitApplicationRequest("x", "", "forklift", 0m, 0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        publisher.Published.Should().BeEmpty();
    }
}
