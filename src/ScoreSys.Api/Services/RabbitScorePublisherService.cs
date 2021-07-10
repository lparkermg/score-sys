using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ScoreSys.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ScoreSys.Api
{
    public sealed class RabbitScorePublisherService : IPublisher<ScoreView>
    {
        private readonly string _exchangeName;
        private IConnection _connection;
        private IQuery<GameView> _query;
        private ILogger<RabbitScorePublisherService> _logger;

        public RabbitScorePublisherService(IConnection connection, string exchangeName, IQuery<GameView> query, ILogger<RabbitScorePublisherService> logger)
        {
            _exchangeName = exchangeName;
            _connection = connection;
            _query = query;
            _logger = logger;
        }

        public async Task<bool> Publish(ScoreView data)
        {
            _logger.LogDebug("Attempting to publish score data.");
            if(data == null)
            {
                throw new ArgumentException("data cannot be null");
            }

            if(data.Id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty");
            }

            if(data.GameId == Guid.Empty)
            {
                throw new ArgumentException("Game Id cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(data.Name))
            {
                throw new ArgumentException("Name cannot be null, empty or whitespace");
            }

            GameView game = null;
            try
            {
                _logger.LogDebug($"Attempting to find game {data.GameId}.");
                game = await _query.Get(data.GameId, 1, 1);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There was an error getting the game.");
                return false;
            }

            if(game == null)
            {
                throw new InvalidOperationException($"Game with id {data.GameId} not found");
            }

            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogDebug("Attempting to send score data message.");
                    using (var model = _connection.CreateModel())
                    {
                        var properties = model.CreateBasicProperties();

                        properties.Type = "score.data";
                        model.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, false, false);
                        byte[] body = Array.Empty<byte>();
                        using (var ms = new MemoryStream())
                        {
                            BinaryWriter bw = new BinaryWriter(ms);
                            bw.Write(data.ToString());
                            body = ms.ToArray();
                        }
                        model.BasicPublish(_exchangeName, "score-data", false, properties, body);

                        return true;
                    }
                }
                catch(Exception e)
                {
                    _logger.LogError(e, "There was an error submitting the score data message.");
                    return false;
                }
            });
        }
    }
}
