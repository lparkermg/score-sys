using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly IQuery<IList<ScoreView>> _queryHandler;

        public ScoreController(IQuery<IList<ScoreView>> queryHandler, ILogger<ScoreController> logger)
        {
            _queryHandler = queryHandler;
            _logger = logger;
        }

        [HttpGet("{gameId}/top")]
        public async Task<IActionResult> GetTop(Guid gameId,[FromQuery]int take, [FromQuery]int skip)
        {
            if(gameId == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var results = await _queryHandler.Get(gameId, take, skip);
                return Ok(results);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                var response = new ObjectResult(e.Message);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return response;
            }
        }

        /*
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
        */
    }
}
