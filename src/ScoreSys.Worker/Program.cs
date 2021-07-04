using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ScoreSys.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();
                    var contextBuilder = new DbContextOptionsBuilder();
                    contextBuilder.UseSqlServer(config["SqlConnection"]);
                    var factory = new ConnectionFactory()
                    {
                        HostName = config["RabbitMQ:host"],
                        UserName = config["RabbitMQ:username"],
                        Password = config["RabbitMQ:password"],
                    };

                    services.AddHostedService<Worker>(s =>
                    new Worker(
                        s.GetRequiredService<ILogger<Worker>>(),
                        contextBuilder.Options,
                        factory.CreateConnection(),
                        config["RabbitMQ:exchange"]));
                });
    }
}
