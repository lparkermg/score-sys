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
            _logger.LogDebug($"Attempting to get top scores for game {gameId}.");
            if(gameId == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                _logger.LogDebug("Querying scores for game.");
                var results = await _queryHandler.Get(gameId, take, skip);
                return Ok(results);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while querying scores for game.");
                var response = new ObjectResult(e.Message);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return response;
            }
        }

        [HttpPost("{gameId}")]
        public async Task<IActionResult> Post(Guid gameId, [FromBody]ScorePost score)
        {
            _logger.LogDebug($"Attempting to submit score for game {gameId}.");
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

            try
            {
                _logger.LogDebug("Attempting to publish score to database.");
                var scoreId = Guid.NewGuid();
                if(!await _publisher.Publish(new ScoreView()
                {
                    Id = scoreId,
                    GameId = gameId,
                    Score = score.Score,
                    Name = score.Name,
                    PostedAt = DateTime.UtcNow,
                }))
                {
                    throw new Exception("Something went wrong while submitting score.");
                }

                return Created("", scoreId);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Error while publishing score to database.");
                var response = new ObjectResult(e.Message);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return response;
            }
        }
    }
}
