using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FireworkServices.Context;
using FireworkServices.Models;
using FireworkServices.Repo;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace FireworkServices
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<FireworkContext>(options =>
                    {
                        options.UseSqlServer(Configuration.GetConnectionString("SQLServer"));
                    }, ServiceLifetime.Transient);
            services.AddTransient<IFireworkRepo, FireworkSQLEnhanceRepo>();
            services.AddSignalR().AddAzureSignalR(Configuration.GetConnectionString("SignalR"));
            services.AddSingleton<FireworkSignalR>();
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(
                    Configuration.GetConnectionString("RedisCache"))
                );
            services.AddSingleton<ServiceBusSender>(x =>
            {
                var client = new ServiceBusClient(Configuration.GetConnectionString("ServicesBus"));
                return client.CreateSender("firework");
            });
            services.AddApplicationInsightsTelemetry(
                Configuration.GetValue<String>("ApplicationInsights:InstrumentationKey") ??
                Configuration.GetValue<String>("APPINSIGHTS_INSTRUMENTATIONKEY")
            );
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = true; });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FireworkServices", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation(
                "Configuring for {Environment} environment",
                env.EnvironmentName);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FireworkServices v1.1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true) // allow any origin
            .AllowCredentials() // allow credentials
            );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<FireworkSignalR>("/fireworkhub");
                endpoints.MapControllers();
            });
        }
    }
}
