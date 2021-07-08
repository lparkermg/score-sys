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
        private string _exchangeName;

        [SetUp]
        public void SetUp()
        {
            _exchangeName = "test-exchange";
            var connection = Mock.Of<IConnection>();
            _publisher = new RabbitScorePublisherService(connection, _exchangeName);
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
    }
}
