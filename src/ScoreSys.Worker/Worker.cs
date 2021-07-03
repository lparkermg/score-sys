using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ScoreSys.Entities;

namespace ScoreSys.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _exchangeName;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly DbContextOptions _contextOptions;

        // TODO: Pass in logger, DBContextOptions and IConnection + add logging for any setup issues.
        public Worker(ILogger<Worker> logger, DbContextOptions contextOptions, string hostName, string username, string password, string exchangeName)
        {
            _logger = logger;
            _exchangeName = exchangeName;
            _factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = username,
                Password = password,
            };
            _connection = _factory.CreateConnection();
            _contextOptions = contextOptions;
        }

        // TODO Wrap in tests/add logging.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout);

                    var queueName = channel.QueueDeclare().QueueName;

                    channel.QueueBind(
                        queue: queueName,
                        exchange: _exchangeName,
                        routingKey: string.Empty);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += EventReceived;
                    channel.BasicConsume(
                        queue: queueName,
                        autoAck: true,
                        consumer: consumer);

                    _logger.LogInformation("Service Started, waiting for scores...");
                    while (!stoppingToken.IsCancellationRequested) { }
                    _logger.LogInformation("Service Stopped...");
                }
            });
        }

        private async void EventReceived(object sender, BasicDeliverEventArgs e)
        {
            var rawBody = e.Body.ToArray();

            // TODO: Validate the scores coming in.
            var body = ScoreViewExtenstions.BytesToScoreView(rawBody);
            using (var context = new ScoreViewContext(_contextOptions))
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                await context.Scores.AddAsync(body);
                await context.SaveChangesAsync();
                transaction.Commit();
                context.Dispose();
            }
            _logger.LogDebug($"Score {body.Id} submitted successfully.");
        }
    }
}
