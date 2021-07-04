using Microsoft.EntityFrameworkCore;
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

        public GameSqlQueryService(DbContextOptions contextOptions) => _contextOptions = contextOptions;

        // TODO: Add logging.
        public async Task<GameView> Get(Guid id, int take = 1, int skip = 1)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Game ID must be provided.");
            }

            if(take != 1 || skip != 1)
            {
                throw new ArgumentException("Take and Skip must be equal to 1.");
            }

            using (var context = new GameViewContext(_contextOptions))
            {
                return await context.FindAsync<GameView>(id);
            }
        }
    }
}
