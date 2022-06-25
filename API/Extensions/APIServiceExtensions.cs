using System.Reflection;
using API.Middlewares;
using FluentValidation.AspNetCore;
using MediatR;
using NJsonSchema;
using NJsonSchema.Generation;
using Sieve.Services;

namespace API.Extensions;

public class SchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        foreach (var (propName, prop) in context.Schema.Properties)
        {
            if (!prop.IsNullable(SchemaType.OpenApi3))
            {
                prop.IsRequired = true;
            }
        }
    }
}

public static class APIServiceExtensions
{
    public static void AddAPIServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRouting(options => options.LowercaseUrls = true);
        services
            .AddControllers(options =>
            {
                // Exception filter.
                options.Filters.Add<ExceptionFilter>();
            })
            .AddFluentValidation(config =>
            {
                config.RegisterValidatorsFromAssembly(Assembly.GetAssembly(typeof(Core.Application)));
            });

        var coreAssembly = Assembly.GetAssembly(typeof(Core.Application));
        if (coreAssembly != null)
        {
            services.AddMediatR(coreAssembly);
            services.AddAutoMapper(coreAssembly);
        }

        services.AddOpenApiDocument(document =>
        {
            document.Title = "TrackTogether";
            document.Description = "REST API schema for TrackTogether's 'Tracking' service.";
            document.DocumentName = "v1";
            
            document.SchemaProcessors.Add(new SchemaProcessor());
        });
        
        services.AddScoped<ISieveProcessor, SieveProcessor>();
    }
}