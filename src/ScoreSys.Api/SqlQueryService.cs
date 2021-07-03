using Microsoft.EntityFrameworkCore;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreSys.Api
{
    public sealed class SqlQueryService : IQuery<ScoreView>
    {
        private readonly DbContextOptions _contextOptions;
        public SqlQueryService(DbContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        // TODO: Add awaitable.
        public async Task<IList<ScoreView>> Get(Guid gameId, int take = 10, int skip = 0)
        {
            using (var context = new ScoreViewContext(_contextOptions))
            {
                return context.Scores.Where(s => s.GameId == gameId).OrderByDescending(s => s.Score).Skip(skip).Take(take).ToList();
            }
        }
    }
}
