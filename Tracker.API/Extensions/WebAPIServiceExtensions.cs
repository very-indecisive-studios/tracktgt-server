using System.Reflection;
using FluentValidation.AspNetCore;
using MediatR;
using Sieve.Services;
using Tracker.API.Middlewares;
using Tracker.Service.Game;

namespace Tracker.API.Extensions;

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
                config.RegisterValidatorsFromAssembly(Assembly.GetAssembly(typeof(Tracker.Core.Application)));
            });

        var coreAssembly = Assembly.GetAssembly(typeof(Tracker.Core.Application));
        if (coreAssembly != null)
        {
            services.AddMediatR(coreAssembly);
            services.AddAutoMapper(coreAssembly);
        }

        services.AddScoped<ISieveProcessor, SieveProcessor>();
    }
}