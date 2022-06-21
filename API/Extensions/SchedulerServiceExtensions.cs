using API.Jobs;
using Quartz;

namespace API.Extensions;

public static class SchedulerServiceExtensions
{
    public static void AddSchedulerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(config =>
        {
            config.SchedulerName = "Scheduler";
            config.SchedulerId = "Main";
            
            config.UseMicrosoftDependencyInjectionJobFactory();

            config.ScheduleJob<FetchWishlistedSwitchGamePricesJob>(trigger => trigger
                .WithIdentity("Fetch wishlisted Switch game prices")
                .StartNow()
                .WithDailyTimeIntervalSchedule(x => x.OnEveryDay().WithIntervalInHours(6)));
        });
        
        services.AddQuartzServer(options =>
        {
            options.AwaitApplicationStarted = true;
            options.WaitForJobsToComplete = false;
        });
    }
}