using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScoreSys.Entities;

namespace ScoreSys.Api.Controllers
{
    // TODO: Wrap in tests and add a Game Controller to add + update games
    [ApiController]
    [Route("[controller]")]
    public class ScoreController : ControllerBase
    {
        private readonly ILogger<ScoreController> _logger;
        private readonly IPublisher<ScoreView> _publisher;
        private readonly IQuery<ScoreView> _queryHandler;

        public ScoreController(ILogger<ScoreController> logger, IPublisher<ScoreView> publisher, IQuery<ScoreView> queryHandler)
        {
            _logger = logger;
            _publisher = publisher;
            _queryHandler = queryHandler;
        }

        [HttpGet("{gameId}/top-{amount}")]
        public async Task<IList<ScoreView>> GetTopTen(Guid gameId, int amount)
        {
            if(amount <= 0)
            {
                return new List<ScoreView>();
            }

            return await _queryHandler.Get(gameId, amount, 0);
        }

        [HttpPost("{gameId}")]
        public IActionResult Post(Guid gameId, ScorePost score)
        {
            _publisher.Publish(new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Name = score.Name,
                Score = score.Score,
                PostedAt = DateTime.UtcNow,
            });
            return Created("", null);
        }
    }
}
