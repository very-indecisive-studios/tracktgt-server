using Serilog;
using API.Extensions;

namespace API;

public class Startup
{
    private readonly IConfiguration _configuration;
        
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLoggerServices();
        services.AddAPIServices(_configuration);
        services.AddDatabaseServices(_configuration);
        services.AddExternalAPIServices(_configuration);
        services.AddSchedulerServices(_configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
        
        app.UseSerilogRequestLogging();
            
        app.UseCors(config =>
        {
            config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });

        // app.UseHttpsRedirection();

        app.UseRouting();

        // app.UseAuthentication();

        // app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
