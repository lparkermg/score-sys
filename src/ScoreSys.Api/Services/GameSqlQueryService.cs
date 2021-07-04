using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreSys.Api.Services
{
    public sealed class GameSqlQueryService : IQuery<GameView>
    {
        private readonly DbContextOptions _contextOptions;
        private readonly ILogger<GameSqlQueryService> _logger;

        public GameSqlQueryService(DbContextOptions contextOptions, ILogger<GameSqlQueryService> logger) 
        { 
            _contextOptions = contextOptions;
            _logger = logger;
        }

        public async Task<GameView> Get(Guid id, int take = 1, int skip = 1)
        {
            _logger.LogDebug($"Attempting to get game {id}.");
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Game ID must be provided.");
            }

            if(take != 1 || skip != 1)
            {
                throw new ArgumentException("Take and Skip must be equal to 1.");
            }

            _logger.LogDebug("Connection to GameView database");
            using (var context = new GameViewContext(_contextOptions))
            {
                _logger.LogDebug("Attempting to find game.");
                return await context.FindAsync<GameView>(id);
            }
        }
    }
}
