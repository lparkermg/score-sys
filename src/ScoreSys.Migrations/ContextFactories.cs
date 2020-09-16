using ScoreSys.Entities;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ScoreSys.Migrations
{
    internal sealed class ContextFactories
    {
        public sealed class ScoreViewContextFactory : DesignTimeContextFactory<ScoreViewContext> { }

        public abstract class DesignTimeContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
        {
            public T CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<T>();
                optionsBuilder.UseSqlServer(
                    ConfigOptions.Views,
                    x => x.MigrationsAssembly(typeof(ContextFactories).Assembly.FullName));

                return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options);
            }
        }
    }
}
