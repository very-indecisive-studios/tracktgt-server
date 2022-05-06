using System.Reflection;
using FluentValidation.AspNetCore;
using MediatR;
using Sieve.Services;
using Tracker.API.Middlewares;
using Tracker.Service.Game;
using Tracker.Service.User;

namespace Tracker.API.Extensions;

public static class ExternalAPIServiceExtentions
{
    public static void AddExternalAPIServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IGameService>(new IGDBAPIService(
            configuration["IGDB:ClientId"], 
            configuration["IGDB:ClientSecret"]));
        
        services.AddSingleton<IUserService>(new FirebaseAPIService(
            configuration["Firebase:CredentialsPath"]));
    }
}