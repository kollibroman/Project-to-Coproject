using ProjectCowork.Api.Controllers;

namespace ProjectCowork.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder AddControllers(this IEndpointRouteBuilder endpoints)
    {
        endpoints.AddProjects();
        endpoints.AddAuth();
        endpoints.MapPostings();
        
        return endpoints;
    }
}