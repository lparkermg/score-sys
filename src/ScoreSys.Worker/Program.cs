using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                    services.AddHostedService<Worker>(s =>
                    new Worker(
                        s.GetRequiredService<ILogger<Worker>>(),
                        contextBuilder.Options,
                        config["RabbitMQ:host"],
                        config["RabbitMQ:username"],
                        config["RabbitMQ:password"],
                        config["RabbitMQ:exchange"]));
                });
    }
}
