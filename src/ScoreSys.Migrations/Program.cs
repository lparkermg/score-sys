using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreSys.Migrations
{
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {
            using (var scoreContext = new ContextFactories.ScoreViewContextFactory().CreateDbContext(args))
            {
                Console.WriteLine($"Migrating Database at '{ConfigOptions.Views}'");
                var pending = (await scoreContext.Database.GetPendingMigrationsAsync()).ToList();

                Console.WriteLine();
                Console.WriteLine("Will run the following migrations:");
                foreach(var pendingMigration in pending)
                {
                    Console.WriteLine(pendingMigration);
                }

                if(!args.Any(a => a.ToLower().Equals("--autoconfirm")))
                {
                    Console.WriteLine();
                    Console.WriteLine("Press 'Y' to confirm, or 'N' to cancel.");

                    while (true)
                    {
                        var entered = Console.ReadKey(true);
                        if (entered.Key == ConsoleKey.Y)
                        {
                            break;
                        }

                        if (entered.Key == ConsoleKey.N)
                        {
                            return;
                        }

                        Console.WriteLine("Invalid Input: Press 'Y' to confirm, or 'N' to cancel.");
                    }
                }

                await scoreContext.Database.MigrateAsync();
            }
        }
    }
}
