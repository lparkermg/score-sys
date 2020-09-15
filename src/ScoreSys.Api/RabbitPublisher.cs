using RabbitMQ.Client;
using ScoreSys.Entities;
using System;
using System.Threading.Tasks;

namespace ScoreSys.Api
{
    public sealed class RabbitPublisher : IPublisher<ScoreView>
    {
        private readonly string _exchangeName;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        public RabbitPublisher(string hostName, string username, string password, string vHost, string exchangeName)
        {
            _exchangeName = exchangeName;
            _factory = new ConnectionFactory() { 
                HostName = hostName,
                UserName = username,
                Password = password,
                VirtualHost = vHost,
            };
            _connection = _factory.CreateConnection();
        }

        public async Task<bool> Publish(ScoreView data)
        {
            return await Task.Run(() =>
             {
                 try
                 {
                     using (var channel = _connection.CreateModel())
                     {
                         channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, true);
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
        }
    }
}
