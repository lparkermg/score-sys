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
        private readonly ILogger<GameController> _logger;

        public GameController(IQuery<GameView> query, ILogger<GameController> logger)
        {
            _logger = logger;
            _query = query;
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
    }
}
