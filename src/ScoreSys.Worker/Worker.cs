using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        private readonly IConnection _connection;
        private readonly DbContextOptions _contextOptions;

        public Worker(ILogger<Worker> logger, DbContextOptions contextOptions, IConnection connection, string exchangeName)
        {
            _logger = logger;
            _exchangeName = exchangeName;
            _connection = connection;
            _contextOptions = contextOptions;
        }

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
                    consumer.Received += HandleEventReceived;
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

        private async void HandleEventReceived(object sender, BasicDeliverEventArgs e)
        {
            switch (e.RoutingKey)
            {
                case "game-data":
                    await HandleSaveGame(e.Body.ToArray());
                    break;
                case "score-data":
                    await HandleSaveScore(e.Body.ToArray());
                    break;
                default:
                    _logger.LogError($"Unknown routing key provided {e.RoutingKey}");
                    break;
            }       
        }

        private async Task HandleSaveScore(byte[] data)
        {
            var view = default(ScoreView);
            using (var memStream = new MemoryStream(data))
            {
                var binReader = new BinaryReader(memStream);
                view.FromString(binReader.ReadString());
            }

            if(view == default(ScoreView))
            {
                _logger.LogError($"Score was not formatted correctly.");
                return;
            }

            using (var context = new ScoreViewContext(_contextOptions))
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                await context.Scores.AddAsync(view);
                await context.SaveChangesAsync();
                transaction.Commit();
                context.Dispose();
            }

            _logger.LogDebug($"Score {view.Id} submitted successfully.");
        }

        private async Task HandleSaveGame(byte[] data)
        {
            var view = default(GameView);

            using (var memStream = new MemoryStream(data))
            {
                var binReader = new BinaryReader(memStream);
                view.FromString(binReader.ReadString());
            }

            if (view == default(GameView))
            {
                _logger.LogError($"Score was not formatted correctly.");
                return;
            }

            using (var context = new GameViewContext(_contextOptions))
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                await context.Games.AddAsync(view);
                await context.SaveChangesAsync();
                transaction.Commit();
                await context.DisposeAsync();
            }

            _logger.LogDebug($"Game {view.Id} submitted successfully.");
        }


    }
}
