using Intake.Worker.Publishing;
using Lab.Intake.Contracts;

namespace Intake.Worker.Endpoints;

public sealed record SubmitApplicationRequest(string ApplicantName, string CompanyNumber, string AssetType, decimal AssetValue, int TermMonths);

public sealed record SubmitApplicationResponse(string ApplicationId);

public static class ApplicationEndpoints
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/intake/applications", async (SubmitApplicationRequest request, IApplicationPublisher publisher, TimeProvider clock, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.CompanyNumber) || request.TermMonths <= 0 || request.AssetValue <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = ["Company number, a positive term and a positive asset value are required."] });
            }

            var applicationId = Guid.NewGuid().ToString();
            await publisher.PublishSubmitted(
                new LeaseApplicationSubmitted
                {
                    ApplicationId = applicationId,
                    ApplicantName = request.ApplicantName,
                    CompanyNumber = request.CompanyNumber,
                    AssetType = request.AssetType,
                    AssetValue = (double)request.AssetValue,
                    TermMonths = request.TermMonths,
                    SubmittedAtUnixMs = clock.GetUtcNow().ToUnixTimeMilliseconds(),
                },
                ct);

            await publisher.PublishEnriched(Enrichment.For(applicationId, request.CompanyNumber), ct);

            return Results.Accepted($"/intake/applications/{applicationId}", new SubmitApplicationResponse(applicationId));
        });

        return app;
    }
}

/// <summary>Stand-in for a company register lookup: derives sector and years trading from the company number.</summary>
public static class Enrichment
{
    private static readonly string[] Sectors = ["construction", "logistics", "agriculture", "manufacturing"];

    public static LeaseApplicationEnriched For(string applicationId, string companyNumber)
    {
        var digits = companyNumber.Where(char.IsDigit).Select(c => c - '0').ToArray();
        var sum = digits.Sum();
        return new LeaseApplicationEnriched
        {
            ApplicationId = applicationId,
            Sector = Sectors[sum % Sectors.Length],
            YearsTrading = 1 + (sum % 25),
        };
    }
}
