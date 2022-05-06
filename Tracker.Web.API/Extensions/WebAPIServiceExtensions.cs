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

        services.AddMediatR(Assembly.GetAssembly(typeof(Tracker.Core.Application)));
        services.AddAutoMapper(Assembly.GetAssembly(typeof(Tracker.Core.Application)));

        services.AddScoped<ISieveProcessor, SieveProcessor>();
    }
}