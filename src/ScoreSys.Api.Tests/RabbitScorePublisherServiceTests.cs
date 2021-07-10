using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSys.Api.Tests
{
    public class RabbitScorePublisherServiceTests
    {
        private RabbitScorePublisherService _publisher;
        private IConnection _connection;
        private Mock<IConnection> _connectionMock;
        private Mock<IModel> _modelMock;
        private Mock<IQuery<GameView>> _gameViewQueryMock;
        private string _exchangeName;
        private Guid _gameId;

        [SetUp]
        public void SetUp()
        {
            _gameId = Guid.NewGuid();
            var gameView = new GameView()
            {
                Name = "Test Game",
                Id = _gameId,
            };
            _exchangeName = "test-exchange";
            _connection = Mock.Of<IConnection>();
            var model = Mock.Of<IModel>();
            var gameSqlQueryService = Mock.Of<IQuery<GameView>>();
            _gameViewQueryMock = Mock.Get(gameSqlQueryService);
            _gameViewQueryMock.Setup(s => s.Get(It.IsAny<Guid>(), 1, 1)).Returns(() => Task.FromResult(gameView));

            _connectionMock = Mock.Get(_connection);
            _modelMock = Mock.Get(model);

            _connectionMock.Setup(c => c.CreateModel()).Returns(model);
            _modelMock.Setup(m => m.CreateBasicProperties()).Returns(Mock.Of<IBasicProperties>());
            var logger = Mock.Of<ILogger<RabbitScorePublisherService>>();
            _publisher = new RabbitScorePublisherService(_connection, _exchangeName, gameSqlQueryService, logger);
        }

        [Test]
        public void Publish_GivenNullScoreView_ThrowsArgumentException()
            => Assert.That(async () => await _publisher.Publish(null), Throws.ArgumentException.With.Message.EqualTo("data cannot be null"));

        [Test]
        public void Publish_GivenScoreViewWithEmptyId_ThrowsArgumentException()
            => Assert.That(async () => await _publisher.Publish(new ScoreView()
            {
                Id = Guid.Empty,
                GameId = Guid.NewGuid(),
                Name = "Test",
                Score = 0,
                PostedAt = DateTime.UtcNow,

            }), Throws.ArgumentException.With.Message.EqualTo("Id cannot be empty"));

        [Test]
        public void Publish_GivenScoreViewWhereGameIdIsEmpty_ThrowsArgumentException()
            => Assert.That(async () => await _publisher.Publish(new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = Guid.Empty,
                Name = "Test",
                Score = 0,
                PostedAt = DateTime.UtcNow,

            }), Throws.ArgumentException.With.Message.EqualTo("Game Id cannot be empty"));

        [TestCase(null)]
        [TestCase("")]
        [TestCase("       ")]
        public void Publish_GivenScoreViewWhereNameIsNullEmptyOrWhitespace_ThrowsArgumentException(string name)
            => Assert.That(async () => await _publisher.Publish(new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                Name = name,
                Score = 0,
                PostedAt = DateTime.UtcNow,

            }), Throws.ArgumentException.With.Message.EqualTo("Name cannot be null, empty or whitespace"));

        [Test]
        public void Publish_GivenScoreViewWhereGameDoesntExist_ThrowsInvalidOperationException()
        {
            var gameSqlQueryService = Mock.Of<IQuery<GameView>>();
            var gameSqlQueryServiceMock = Mock.Get(gameSqlQueryService);
            gameSqlQueryServiceMock.Setup(s => s.Get(It.IsAny<Guid>(), 1, 1)).Returns(() => Task.FromResult((GameView)null));
            var logger = Mock.Of<ILogger<RabbitScorePublisherService>>();
            var publisher = new RabbitScorePublisherService(_connection, "exchange", gameSqlQueryService, logger);
            var gameId = Guid.NewGuid();

            Assert.That(async () => await publisher.Publish(new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Name = "Test User",
                Score = 0,
                PostedAt = DateTime.UtcNow,
            }), Throws.InvalidOperationException.With.Message.EqualTo($"Game with id {gameId} not found"));
        }

        [Test]
        public async Task Publish_GivenValidScoreView_CorrectlyPublishesViewDataAndReturnsTrue()
        {
            var score = new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = _gameId,
                Name = "Test User",
                Score = 5,
                PostedAt = DateTime.UtcNow,
            };

            Assert.That(await _publisher.Publish(score), Is.True);

            _connectionMock.Verify(c => c.CreateModel(), Times.Once, "CreateModel was not called once.");
            _modelMock.Verify(m => m.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, false, false, null), Times.Once, "ExchangeDeclare was not called once.");
            _modelMock.Verify(m => m.BasicPublish(_exchangeName, "score-data", false, It.IsNotNull<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()), Times.Once, "BasicPublish was not called correctly.");
        }

        [Test]
        public async Task Publish_GivenConnectionThatThrows_ReturnsFalse()
        {
            var score = new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = _gameId,
                Name = "Test User",
                Score = 5,
                PostedAt = DateTime.UtcNow,
            };

            _connectionMock.Setup(c => c.CreateModel()).Throws<Exception>();

            Assert.That(await _publisher.Publish(score), Is.False);
        }

        [Test]
        public async Task Publish_GivenQueryServiceThatThrows_ReturnsFalse()
        {
            var score = new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = _gameId,
                Name = "Test User",
                Score = 5,
                PostedAt = DateTime.UtcNow,
            };

            _gameViewQueryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Throws<Exception>();

            Assert.That(await _publisher.Publish(score), Is.False);
        }

        [Test]
        public async Task Publish_GivenModelThatThrows_ReturnsFalse()
        {
            var score = new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = _gameId,
                Name = "Test User",
                Score = 5,
                PostedAt = DateTime.UtcNow,
            };

            _modelMock.Setup(m => m.CreateBasicProperties()).Throws<Exception>();

            Assert.That(await _publisher.Publish(score), Is.False);
        }
    }
}
