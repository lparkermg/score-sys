using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ScoreSys.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IQuery<GameView> _query;
        private readonly IPublisher<GameView> _publisher;
        private readonly ILogger<GameController> _logger;

        public GameController(IQuery<GameView> query, IPublisher<GameView> publisher, ILogger<GameController> logger)
        {
            _logger = logger;
            _query = query;
            _publisher = publisher;
        }

        [HttpGet("{gameId}")]
        public async Task<IActionResult> Get(Guid gameId)
        {
            _logger.LogDebug($"Get with GameId {gameId} started.");
            if (gameId == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                _logger.LogDebug($"Querying {typeof(IQuery<GameView>)}");
                var result = await _query.Get(gameId, 1, 1);

                return result == null ? NotFound($"Game with id {gameId} not found") : Ok(result);
            }
            catch(ArgumentException ae)
            {
                _logger.LogDebug(ae, $"{typeof(IQuery<GameView>)} threw {typeof(ArgumentException)}.");
                return BadRequest(ae.Message);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(IQuery<GameView>)} failed in an unexpected way.");
                var response = new ObjectResult("There was an error requesting the game.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return response;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(GamePost game)
        {
            if (game == null)
            {
                return BadRequest("No data provided.");
            }

            if (string.IsNullOrWhiteSpace(game.Name))
            {
                return BadRequest($"Invalid {nameof(game.Name)} provided.");
            }

            try
            {
                var id = Guid.NewGuid();
                var response = await _publisher.Publish(new GameView()
                {
                    Name = game.Name,
                    Id = id,
                });

                if (!response)
                {
                    var objResponse = new ObjectResult("Something went wrong saving the new game.");
                    objResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return objResponse;
                }

                return Created(id.ToString(), null);
            }
            catch(ArgumentException ae)
            {
                _logger.LogDebug(ae, $"{typeof(IPublisher<GameView>)} threw {typeof(ArgumentException)}.");
                return BadRequest(ae.Message);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"{typeof(IPublisher<GameView>)} failed in an unexpected way.");
                var response = new ObjectResult("Something went wrong saving the new game.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return response;
            }
        }
    }
}
