using BrokerPulse.Api.Common.Extensions;

namespace BrokerPulse.Api.Features.Health;

public class HealthEndpoints : IEndpointGroup
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok())
            .WithName("GetHealth");
    }
}
