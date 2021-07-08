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

        // TODO: Pass in the IConnection itself + Add Logging.
        public RabbitScorePublisherService(IConnection connection, string exchangeName, IQuery<GameView> query)
        {
            _exchangeName = exchangeName;
            _connection = connection;
            _query = query;
        }

        // TODO: Wrap in tests + Add Logging 
        /*public async Task<bool> Publish(ScoreView data)
        {
            return await Task.Run(() =>
             {
                 try
                 {
                     using (var channel = _connection.CreateModel())
                     {
                         channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, false);
                         var body = data.ToBytes();
                         channel.BasicPublish(_exchangeName, string.Empty, basicProperties: null, body: body);

                         return true;
                     }
                 }
                 catch (Exception e)
                 {
                     return false;
                 }
             });
        }*/

        public async Task<bool> Publish(ScoreView data)
        {
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

            var game = _query.Get(data.GameId, 1, 1);

            if(game == null)
            {
                throw new InvalidOperationException($"Game with id {data.GameId} not found");
            }

            return await Task.Run(() =>
            {
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
            });
        }
    }
}
