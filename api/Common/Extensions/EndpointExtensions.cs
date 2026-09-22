namespace BrokerPulse.Api.Common.Extensions;

public interface IEndpointGroup
{
    void MapEndpoints(IEndpointRouteBuilder app);
}

public static class EndpointExtensions
{
    public static void MapEndpointGroups(this WebApplication app)
    {
        var endpointGroupType = typeof(IEndpointGroup);
        var endpointGroups = endpointGroupType.Assembly
            .GetTypes()
            .Where(t => endpointGroupType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IEndpointGroup>();

        foreach (var group in endpointGroups)
        {
            group.MapEndpoints(app);
        }
    }
}
