using Microsoft.EntityFrameworkCore;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreSys.Api
{
    public sealed class ScoreSqlQueryService : IQuery<IList<ScoreView>>
    {
        private readonly DbContextOptions _contextOptions;
        public ScoreSqlQueryService(DbContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public async Task<IList<ScoreView>> Get(Guid gameId, int take = 10, int skip = 0)
        {
            if (gameId == Guid.Empty)
            {
                throw new ArgumentException("Game Id cannot be empty");
            }

            if (take <= 0)
            {
                throw new ArgumentException("Take amount must be above 0");
            }

            if(skip < 0)
            {
                throw new ArgumentException("Skip amnount must be 0 or above");
            }

            return await Task.Run(() =>
            {
                using (var context = new ScoreViewContext(_contextOptions))
                {
                    return context.Scores.Where(s => s.GameId == gameId).OrderByDescending(s => s.Score).Skip(skip).Take(take).ToList();
                }
            });
        }
    }
}
