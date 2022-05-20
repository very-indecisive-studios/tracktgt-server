using System.Reflection;
using API.Middlewares;
using FluentValidation.AspNetCore;
using MediatR;
using Sieve.Services;

namespace API.Extensions;

public static class WebAPIServiceExtensions
{
    public static void AddWebAPIServices(this IServiceCollection services, IConfiguration configuration)
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

        services.AddScoped<ISieveProcessor, SieveProcessor>();
    }
}