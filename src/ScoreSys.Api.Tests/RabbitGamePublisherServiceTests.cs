using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using ScoreSys.Api.Services;
using ScoreSys.Entities;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace ScoreSys.Api.Tests
{
    public class RabbitGamePublisherServiceTests
    {
        private RabbitGamePublisherService _publisher;
        private Mock<IConnection> _connectionMock;
        private Mock<IModel> _modelMock;
        private ILogger<RabbitGamePublisherService> _logger;
        private string _exchangeName;

        [SetUp]
        public void SetUp()
        {
            _exchangeName = "test-exchange";
            var connection = Mock.Of<IConnection>();
            var model = Mock.Of<IModel>();

            _logger = Mock.Of<ILogger<RabbitGamePublisherService>>();
            _connectionMock = Mock.Get(connection);
            _modelMock = Mock.Get(model);
            _connectionMock.Setup(c => c.CreateModel()).Returns(model);
            _modelMock.Setup(m => m.CreateBasicProperties()).Returns(Mock.Of<IBasicProperties>());
            _publisher = new RabbitGamePublisherService(connection, _exchangeName, _logger);
        }

        [Test]
        public void Publish_GivenNullGameView_ThrowsArgumentException()
            => Assert.That(async () => await _publisher.Publish(null), Throws.ArgumentException.With.Message.EqualTo("data cannot be null"));

        [Test]
        public void Publish_GivenGameViewWithEmptyId_ThrowsArgumentException()
            => Assert.That(async () => await _publisher.Publish(new GameView()
            {
                Id = Guid.Empty,
                Name = "Test Name",
            }), Throws.ArgumentException.With.Message.EqualTo("Id cannot be empty"));

        [TestCase("")]
        [TestCase(null)]
        [TestCase("      ")]
        public void Publish_GivenGameViewWithInvalidGameName_ThrowsArgumentException(string name)
            => Assert.That(async () => await _publisher.Publish(new GameView()
            {
                Id = Guid.NewGuid(),
                Name = name,
            }), Throws.ArgumentException.With.Message.EqualTo("Name cannot be null, empty or whitespace"));

        [Test]
        public async Task Publish_GivenValidGameView_CorrectlyPublishesViewDataAndReturnsTrue()
        {
            var view = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Test Name",
            };
            Assert.That(await _publisher.Publish(view), Is.True);

            _connectionMock.Verify(c => c.CreateModel(), Times.Once, "CreateModel was not called once.");
            _modelMock.Verify(m => m.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, false, true, null), Times.Once, "ExchangeDeclare was not called once.");
            _modelMock.Verify(m => m.BasicPublish(_exchangeName, "game-data", false, It.IsNotNull<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()), Times.Once, "BasicPublish was not called correctly.");
        }

        [Test]
        public async Task Publish_GivenConnectionThatThrows_ReturnsFalse()
        {
            var view = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Test Name",
            };
            _connectionMock.Setup(c => c.CreateModel()).Throws<Exception>();
            Assert.That(await _publisher.Publish(view), Is.False);
        }

        [Test]
        public async Task Publish_GivenModelThatThrows_ReturnsFalse()
        {
            var connection = Mock.Of<IConnection>();
            var model = Mock.Of<IModel>();

            var connectionMock = Mock.Get(connection);
            var modelMock = Mock.Get(model);
            connectionMock.Setup(c => c.CreateModel()).Returns(model);
            modelMock.Setup(m => m.CreateBasicProperties()).Throws<Exception>();

            var publisher = new RabbitGamePublisherService(connection, _exchangeName, _logger);
            var view = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Test Name",
            };

            Assert.That(await publisher.Publish(view), Is.False);
        }
    }
}
