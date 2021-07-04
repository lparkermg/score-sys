using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ScoreSys.Api.Services
{
    public class RabbitGamePublisherService : IPublisher<GameView>
    {
        private readonly IConnection _connection;
        private readonly string _exchangeName;
        private readonly ILogger<RabbitGamePublisherService> _logger;

        public RabbitGamePublisherService(IConnection connection, string exchangeName, ILogger<RabbitGamePublisherService> logger) 
        { 
            _connection = connection;
            _exchangeName = exchangeName;
            _logger = logger;
        }

        public async Task<bool> Publish(GameView data)
        {
            _logger.LogDebug("Starting GameView publish.");
            if (data == null)
            {
                throw new ArgumentException("data cannot be null");
            }

            if (data.Id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(data.Name))
            {
                throw new ArgumentException("Name cannot be null, empty or whitespace");
            }

            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogDebug("Attempting to publish GameView message.");
                    using (var model = _connection.CreateModel())
                    {
                        var properties = model.CreateBasicProperties();

                        properties.Type = "game.data";
                        model.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, false, true);
                        byte[] body = Array.Empty<byte>();
                        using (var ms = new MemoryStream())
                        {
                            BinaryWriter bw = new BinaryWriter(ms);
                            bw.Write(data.ToString());
                            body = ms.ToArray();
                        }
                        model.BasicPublish(_exchangeName, "game-data-queue", false, properties, body);

                        _logger.LogDebug("GameView message successwfully published.");
                        return true;
                    }
                }
                catch(Exception e)
                {
                    _logger.LogError(e, "Failed to publish game data.");
                    return false;
                }
            });
        }
    }
}
