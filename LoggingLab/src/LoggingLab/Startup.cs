using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Glimpse;
using Glimpse.Agent;
using Glimpse.Initialization;
using Serilog;
using Serilog.Events;
using JSNLog;

namespace LoggingLab
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel
                .Debug()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("AppCode", "12345")
                .WriteTo.RollingFile(@"C:\Data\log-{Date}.txt", LogEventLevel.Warning, "{Timestamp:HH:mm:ss} [{Level}] [{AppCode}] [{MachineName}] [{EnvironmentUserName}] [{ThreadId}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .WriteTo.Seq("http://localhost:5341/")
                .CreateLogger();
            //TODO: el RollingFile no es asíncrono, así que se tiene que encadenar con el sink "async").
            //TODO: enrichers custom para las excepciones: callStack, InnerException, ...
            //.WriteTo.LiterateConsole(LogEventLevel.Information, "{Timestamp:HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            //services.AddGlimpse();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            loggerFactory.AddSerilog();
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                //app.UseGlimpse();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Configure JSNLog
            var jsnlogConfiguration = new JsnlogConfiguration();
            app.UseJSNLog(new LoggingAdapter(loggerFactory), jsnlogConfiguration);

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

    }
}
