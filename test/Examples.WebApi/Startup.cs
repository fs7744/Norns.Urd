using Examples.WebApi.Controllers;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Norns.Urd;
using System.Reflection;

namespace Examples.WebApi
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
            services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblies(new Assembly[] { typeof(Startup).Assembly })).AddControllersAsServices().AddXmlSerializerFormatters();
            services.AddTransient<IAopTest, AopTest>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApp1", Version = "v1" });
            });
            services.AddSingleton<ITestClient>();
            services.ConfigureAop(i => 
            {
                i.GlobalInterceptors.Add(new ConsoleInterceptor());
                i.EnableHttpClient();
                i.EnableMemoryCache();
                //i.NonPredicates.AddNamespace("FluentValidation.*");
                //i.NonPredicates.AddService("Examples.WebApi.Controllers.ForValidatorDemoIn");
                //i.NonPredicates.AddService("Examples.WebApi.Controllers.NotificationEditInValidator");
            });
            services.AddHttpClientNewtonsoftJosn();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
                                 "WebApp1 v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}