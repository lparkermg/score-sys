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
    // TODO: Add logging
    [ApiController]
    [Route("[controller]")]
    public class ScoreController : ControllerBase
    {
        private readonly ILogger<ScoreController> _logger;
        private readonly IQuery<IList<ScoreView>> _queryHandler;
        private readonly IPublisher<ScoreView> _publisher;

        public ScoreController(IQuery<IList<ScoreView>> queryHandler, IPublisher<ScoreView> publisher, ILogger<ScoreController> logger)
        {
            _queryHandler = queryHandler;
            _publisher = publisher;
            _logger = logger;
        }

        [HttpGet("{gameId}/top")]
        public async Task<IActionResult> GetTop(Guid gameId, [FromQuery]int take, [FromQuery]int skip)
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

        [HttpPost("{gameId}")]
        public async Task<IActionResult> Post(Guid gameId, [FromBody]ScorePost score)
        {
            if(gameId == Guid.Empty)
            {
                return BadRequest("Game Id cannot be null");
            }

            if(score == null)
            {
                return BadRequest("Request body is required");
            }

            if (string.IsNullOrWhiteSpace(score.Name))
            {
                return BadRequest("Name must be populated");
            }

            if (score.Score < 0)
            {
                return BadRequest("Score cannot be negative");
            }

            var scoreId = Guid.NewGuid();
            await _publisher.Publish(new ScoreView()
            { 
                Id = scoreId,
                GameId = gameId,
                Score = score.Score,
                Name = score.Name,
                PostedAt = DateTime.UtcNow,
            });
            return Created("", scoreId);
        }
    }
}
