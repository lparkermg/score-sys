using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ScoreSys.Api.Controllers;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSys.Api.Tests.Controllers
{
    public class ScoreControllerTests
    {
        private ILogger<ScoreController> _logger;
        private Mock<IQuery<IList<ScoreView>>> _queryMock;
        private ScoreController _controller;

        [SetUp]
        public void SetUp()
        {
            _logger = Mock.Of<ILogger<ScoreController>>();
            var query = Mock.Of<IQuery<IList<ScoreView>>>();
            _queryMock = Mock.Get(query);
            _controller = new ScoreController(query, _logger);
        }

        [Test]
        public async Task GetTop_GivenEmptyGameId_ReturnsBadRequest()
        {
            var result = await _controller.GetTop(Guid.Empty, 1, 1);
            var badRequest = result as BadRequestResult;

            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetTop_GivenGameId_CallsQueryWithCorrectId()
        {
            var id = Guid.NewGuid();
            await _controller.GetTop(id, 1, 1);
            _queryMock.Verify(q => q.Get(id, It.IsAny<int>(), It.IsAny<int>()), Times.Once, "Get with specified id not only called once.");
        }

        [Test]
        public async Task GetTop_GivenTake_CallsQueryWithCorrectTakeValue()
        {
            await _controller.GetTop(Guid.NewGuid(), 5, 1);
            _queryMock.Verify(q => q.Get(It.IsAny<Guid>(), 5, It.IsAny<int>()), Times.Once, "Get with specified take not only called once.");
        }

        [Test]
        public async Task GetTop_GivenSkip_CallsQueryWithCorrectSkipValue()
        {
            var id = Guid.NewGuid();
            await _controller.GetTop(Guid.NewGuid(),5, 2);
            _queryMock.Verify(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), 2), Times.Once, "Get with specified skip not only called once.");
        }

        [Test]
        public async Task GetTop_GivenQueryReturningNothing_ReturnsOkWithEmptyList()
        {
            _queryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<ScoreView>());
            var result = await _controller.GetTop(Guid.NewGuid(), 1, 1);
            var ok = result as OkObjectResult;
            Assert.That(ok.Value, Is.TypeOf<List<ScoreView>>());
            Assert.That(ok.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));

            var resultValue = (List<ScoreView>)ok.Value;
            Assert.That(resultValue, Is.Empty);
        }

        [Test]
        public async Task GetTop_GivenQueryReturningResults_ReturnsOkWithScores()
        {
            var scores = new List<ScoreView>()
            {
                BuildScore(),
                BuildScore(),
                BuildScore(),
                BuildScore()
            };

            _queryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(scores);

            var result = await _controller.GetTop(Guid.NewGuid(), 4, 0);
            var ok = result as OkObjectResult;

            Assert.That(ok.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
            Assert.That(ok.Value, Is.TypeOf<List<ScoreView>>());
            var resultValue = (List<ScoreView>)ok.Value;
            Assert.That(resultValue, Is.EquivalentTo(scores).Using<ScoreView, ScoreView>((a, e) =>
                    a.Id == e.Id &&
                    a.GameId == e.GameId &&
                    a.Name == e.Name &&
                    a.Score == e.Score &&
                    a.PostedAt == e.PostedAt));
        }

        [Test]
        public async Task GetTop_GivenQueryThrowingArgumentException_ReturnsBadRequestWithMessage()
        {
            var message = "An error happened";
            _queryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new ArgumentException(message));
            var result = await _controller.GetTop(Guid.NewGuid(), 1, 1);
            var badRequest = result as BadRequestObjectResult;

            Assert.That(badRequest.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(badRequest.Value.ToString(), Is.EqualTo(message));
        }

        [Test]
        public async Task GetTop_GivenQueryThrowingException_ReturnsInternalServerErrorCodeWithMessage()
        {
            var message = "An error happened";
            _queryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception(message));
            var result = await _controller.GetTop(Guid.NewGuid(), 1, 1);
            var error = result as ObjectResult;

            Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(error.Value.ToString(), Is.EqualTo(message));
        }

        private ScoreView BuildScore()
            => new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                Score = 5,
                Name = "Test Name",
                PostedAt = DateTime.UtcNow,
            };
    }
}
