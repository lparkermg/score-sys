using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ScoreSys.Api.Controllers;
using ScoreSys.Api.Services;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSys.Api.Tests.Controllers
{
    public class GameControllerTests
    {
        private ILogger<GameController> _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = Mock.Of<ILogger<GameController>>();
        }

        [Test]
        public async Task Get_GivenEmptyId_ReturnsBadRequest()
        {
            var controller = new GameController(null, null, _logger);
            var result = await controller.Get(Guid.Empty);
            var badRequest = result as BadRequestResult;

            Assert.That(badRequest, Is.Not.Null);

            Assert.That(badRequest.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Get_WithQueryNotFound_ReturnsNotFound()
        {
            var notFoundQuery = Mock.Of<IQuery<GameView>>();
            var notFoundQueryMock = Mock.Get(notFoundQuery);
            notFoundQueryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.Run(() => (GameView)null));

            var controller = new GameController(notFoundQuery, null, _logger);

            var id = Guid.NewGuid();
            var result = await controller.Get(id);
            var notFound = result as NotFoundObjectResult;

            Assert.That(notFound, Is.Not.Null);

            Assert.That(notFound.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
            Assert.That(notFound.Value.ToString(), Is.EqualTo($"Game with id {id} not found"));
        }

        [Test]
        public async Task Get_WithValidReturningQuery_ReturnsOkWithObject()
        {
            var gameView = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Game Name",
            };

            var okQuery = Mock.Of<IQuery<GameView>>();
            var okQueryMock = Mock.Get(okQuery);
            okQueryMock.Setup(q => q.Get(gameView.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(Task.Run(() => gameView));

            var controller = new GameController(okQuery, null, _logger);

            var result = await controller.Get(gameView.Id);
            var ok = result as OkObjectResult;

            Assert.That(ok, Is.Not.Null);

            Assert.That(ok.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
            Assert.That(ok.Value, Is.TypeOf<GameView>());
            var okValue = ok.Value as GameView;

            Assert.That(okValue, Is.EqualTo(gameView).Using<GameView, GameView>((a, e) => a.Id == e.Id && a.Name == e.Name));
        }

        [Test]
        public async Task Get_WithQueryServiceThrowingArgumentException_ReturnsBadRequestWithCorrectMessage()
        {
            var exceptionMessage = "I threw this message";

            var throwingQuery = Mock.Of<IQuery<GameView>>();
            var throwingQueryMock = Mock.Get(throwingQuery);
            throwingQueryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new ArgumentException(exceptionMessage));

            var controller = new GameController(throwingQuery, null, _logger);

            var result = await controller.Get(Guid.NewGuid());

            var badRequest = result as BadRequestObjectResult;

            Assert.That(badRequest, Is.Not.Null);

            Assert.That(badRequest.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(badRequest.Value.ToString(), Is.EqualTo(exceptionMessage));
        }

        [Test]
        public async Task Get_WithQueryServiceThrowingException_ReturnsInternalServerErrorWithGenericMessage()
        {
            var exceptionMessage = "I threw this message";

            var throwingQuery = Mock.Of<IQuery<GameView>>();
            var throwingQueryMock = Mock.Get(throwingQuery);
            throwingQueryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception(exceptionMessage));

            var controller = new GameController(throwingQuery, null, _logger);

            var result = await controller.Get(Guid.NewGuid());
            var serverError = result as ObjectResult;

            Assert.That(serverError, Is.Not.Null);

            Assert.That(serverError.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(serverError.Value.ToString(), Is.EqualTo("There was an error requesting the game."));
        }

        [Test]
        public async Task Post_WithNullGamePost_ReturnsBadRequestWithMessage()
        {
            var controller = new GameController(null, null, _logger);
            var response = await controller.Post(null);

            var badRequest = response as BadRequestObjectResult;

            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(badRequest.Value.ToString(), Is.EqualTo("No data provided."));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("      ")]
        public async Task Post_WithInvalidName_ReturnsBadRequestWithMessage(string name)
        {
            var model = new GamePost()
            {
                Name = name,
            };

            var controller = new GameController(null, null, _logger);
            var response = await controller.Post(model);

            var badRequest = response as BadRequestObjectResult;

            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(badRequest.Value.ToString(), Is.EqualTo($"Invalid {nameof(model.Name)} provided."));
        }

        [Test]
        public async Task Post_WithValidModel_CallsPublishAndReturnsCreatedWithIdInLocationHeader()
        {
            var model = new GamePost()
            {
                Name = "Test Game",
            };

            var publisher = Mock.Of<IPublisher<GameView>>();
            var publisherMock = Mock.Get(publisher);
            publisherMock.Setup(p => p.Publish(It.IsAny<GameView>())).ReturnsAsync(true);
            var controller = new GameController(null, publisher, _logger);

            var response = await controller.Post(model);
            var created = response as CreatedResult;

            Assert.That(created, Is.Not.Null);
            Assert.That(created.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
            Assert.That(created.Location, Is.Not.Null);
        }

        [Test]
        public async Task Post_WithPublisherReturningFalse_ReturnsInternalServerErrorWithMessage()
        {
            var model = new GamePost()
            {
                Name = "Test Game",
            };

            var publisher = Mock.Of<IPublisher<GameView>>();
            var publisherMock = Mock.Get(publisher);
            publisherMock.Setup(p => p.Publish(It.IsAny<GameView>())).ReturnsAsync(false);

            var controller = new GameController(null, publisher, _logger);

            var response = await controller.Post(model);
            var error = response as ObjectResult;

            Assert.That(error, Is.Not.Null);
            Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(error.Value.ToString(), Is.EqualTo("Something went wrong saving the new game."));
        }

        [Test]
        public async Task Post_WithPublisherThrowingArgumentException_ReturnsBadRequestWithExceptionMessage()
        {
            var model = new GamePost()
            {
                Name = "Test Game",
            };

            var exception = "I threw this exception.";

            var publisher = Mock.Of<IPublisher<GameView>>();
            var publisherMock = Mock.Get(publisher);
            publisherMock.Setup(p => p.Publish(It.IsAny<GameView>())).Throws(new ArgumentException(exception));

            var controller = new GameController(null, publisher, _logger);

            var response = await controller.Post(model);
            var error = response as BadRequestObjectResult;

            Assert.That(error, Is.Not.Null);
            Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(error.Value.ToString(), Is.EqualTo(exception));
        }

        [Test]
        public async Task Post_WithPublisherThrowingException_ReturnsInternalServerErrorWithMessage()
        {
            var model = new GamePost()
            {
                Name = "Test Game",
            };

            var publisher = Mock.Of<IPublisher<GameView>>();
            var publisherMock = Mock.Get(publisher);
            publisherMock.Setup(p => p.Publish(It.IsAny<GameView>())).Throws<Exception>();

            var controller = new GameController(null, publisher, _logger);

            var response = await controller.Post(model);
            var error = response as ObjectResult;

            Assert.That(error, Is.Not.Null);
            Assert.That(error.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(error.Value.ToString(), Is.EqualTo("Something went wrong saving the new game."));
        }
    }
}
