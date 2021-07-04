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

        public RabbitGamePublisherService(IConnection connection, string exchangeName) 
        { 
            _connection = connection;
            _exchangeName = exchangeName;
        }

        public async Task<bool> Publish(GameView data)
        {
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

                        return true;
                    }
                }
                catch(Exception e)
                {
                    return false;
                }
            });
        }
    }
}
