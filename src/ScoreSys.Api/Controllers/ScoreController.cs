using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPublisher<ScoreView> _publisher;

        public ScoreController(ILogger<ScoreController> logger, IPublisher<ScoreView> publisher)
        {
            _logger = logger;
            _publisher = publisher;
        }

        [HttpGet]
        public IEnumerable<ScoreView> Get()
        {
            return new List<ScoreView>();
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
                PostedAt = DateTime.Now,
            });
            return Created("", null);
        }
    }
}
