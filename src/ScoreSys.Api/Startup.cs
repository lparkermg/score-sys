using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScoreSys.Entities;

namespace ScoreSys.Api
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
            // TODO: Add environment variable capturing.
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();
            var contextBuilder = new DbContextOptionsBuilder();
            contextBuilder.UseSqlServer(config["SqlConnection"]);
            services.AddSingleton<IPublisher<ScoreView>>(new RabbitScorePublisherService(
                config["RabbitMQ:host"],
                config["RabbitMQ:username"],
                config["RabbitMQ:password"],
                config["RabbitMQ:exchange"]));
            services.AddSingleton<IQuery<IList<ScoreView>>>(new ScoreSqlQueryService(contextBuilder.Options));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
