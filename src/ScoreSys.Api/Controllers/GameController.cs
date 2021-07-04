using Microsoft.AspNetCore.Mvc;
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

        // TODO: Add logging and test (Should be able to do this by mocking the logger).
        public GameController(IQuery<GameView> query) => _query = query;

        [HttpGet("{gameId}")]
        public async Task<IActionResult> Get(Guid gameId)
        {
            if (gameId == Guid.Empty)
            {
                return BadRequest();
            }
            try
            {
                var result = await _query.Get(gameId, 1, 1);

                return result == null ? NotFound($"Game with id {gameId} not found") : Ok(result);
            }
            catch(ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch(Exception)
            {
                var response = new ObjectResult("There was an error requesting the game.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return response;
            }
        }
    }
}
