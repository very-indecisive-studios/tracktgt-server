namespace Tracker.API.Extensions;

public static class SwaggerServiceExtensions
{
    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddOpenApiDocument(document =>
        {
            document.DocumentName = "v1";
        });
    }
}