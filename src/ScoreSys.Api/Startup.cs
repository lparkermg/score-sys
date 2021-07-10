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
using RabbitMQ.Client;
using ScoreSys.Api.Services;
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
            var factory = new ConnectionFactory()
            {
                HostName = config["RabbitMQ:host"],
                UserName = config["RabbitMQ:username"],
                Password = config["RabbitMQ:password"],
            };
            var exchangeName = config["RabbitMQ:exchange"];

#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
            var sp = services.BuildServiceProvider();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'

            services.AddSingleton<IPublisher<GameView>>(new RabbitGamePublisherService(factory.CreateConnection(), exchangeName, sp.GetService<ILogger<RabbitGamePublisherService>>()));
            services.AddSingleton<IQuery<GameView>>(new GameSqlQueryService(contextBuilder.Options, sp.GetService<ILogger<GameSqlQueryService>>()));

#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
            // This needs to be called again to update the provider with the added services.
            sp = services.BuildServiceProvider();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'

            services.AddSingleton<IPublisher<ScoreView>>(new RabbitScorePublisherService(factory.CreateConnection(), exchangeName, sp.GetService<IQuery<GameView>>(), sp.GetService<ILogger<RabbitScorePublisherService>>()));
            services.AddSingleton<IQuery<IList<ScoreView>>>(new ScoreSqlQueryService(contextBuilder.Options));
            services.AddSwaggerGen();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Score Sys API");
            });

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
