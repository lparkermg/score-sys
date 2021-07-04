using Microsoft.AspNetCore.Mvc;
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
        [Test]
        public async Task Get_GivenEmptyId_ReturnsBadRequest()
        {
            var controller = new GameController(null);
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
            var controller = new GameController(notFoundQuery);
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
            okQueryMock.Setup(q => q.Get(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.Run(() => gameView));
            var controller = new GameController(okQuery);
            var id = Guid.NewGuid();
            var result = await controller.Get(id);
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

            var controller = new GameController(throwingQuery);
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

            var controller = new GameController(throwingQuery);
            var result = await controller.Get(Guid.NewGuid());
            var serverError = result as ObjectResult;

            Assert.That(serverError, Is.Not.Null);

            Assert.That(serverError.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(serverError.Value.ToString(), Is.EqualTo("There was an error requesting the game."));

        }
    }
}
