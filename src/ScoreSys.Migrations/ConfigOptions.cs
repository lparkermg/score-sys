using Microsoft.Extensions.Configuration;

namespace ScoreSys.Migrations
{
    public static class ConfigOptions
    {
        private static readonly IConfigurationRoot Config;
        static ConfigOptions()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("config.json", false, false);
            Config = builder.Build();
        }

        public static string Views => Config["SqlViewConnectionString"];
    }
}
