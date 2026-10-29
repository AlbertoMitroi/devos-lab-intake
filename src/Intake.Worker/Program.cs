using Intake.Worker.Endpoints;
using Intake.Worker.Publishing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddIntakeKafka(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapApplicationEndpoints();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
