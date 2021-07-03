using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Microsoft.Data.Sqlite;
using ScoreSys.Api.Services;
using System;
using System.Threading.Tasks;
using ScoreSys.Entities;

namespace ScoreSys.Api.Tests
{
    public class GameSqlQueryServiceTests
    {
        private GameSqlQueryService _service;
        private DbContextOptions _contextOptions;
        private SqliteConnection _connection;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _contextOptions = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
            _service = new GameSqlQueryService(_contextOptions);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _connection.CloseAsync();
        }

        [Test]
        public void Get_GivenEmptyId_ThrowsArgumentException()
            => Assert.That(() => _service.Get(Guid.Empty), Throws.ArgumentException.With.Message.EqualTo("Game ID must be provided."));

        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(0, 1)]
        [TestCase(2, 1)]
        [TestCase(1, 2)]
        public void Get_GivenInvalidSkipOrTake_ThrowsArgumentException(int take, int skip)
            => Assert.That(() => _service.Get(Guid.NewGuid(), take, skip), Throws.ArgumentException.With.Message.EqualTo("Take and Skip must be equal to 1."));

        [Test]
        public async Task Get_GivenNonExistantGameId_ReturnsNull()
        {
            var testView = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Test Game",
            };

            var context = new GameViewContext(_contextOptions);
            await _connection.OpenAsync();
            context.Database.EnsureCreated();
            await context.AddAsync(testView);

            await context.SaveChangesAsync();

            Assert.That(await _service.Get(Guid.NewGuid()), Is.Null);
        }

        [Test]
        public async Task Get_GivenId_ReturnsCorrectView()
        {
            var expectedView = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Test Game",
            };

            var context = new GameViewContext(_contextOptions);
            await _connection.OpenAsync();
            context.Database.EnsureCreated();
            await context.AddAsync(expectedView);

            await context.SaveChangesAsync();

            Assert.That(await _service.Get(expectedView.Id), Is.EqualTo(expectedView)
                .Using<GameView, GameView>((a, e) => a.Id == e.Id && a.Name == e.Name));
        }

        [Test]
        public async Task Get_GivenIdWithMultipleViews_ReturnsCorrectView()
        {
            var firstView = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Another Game",
            };

            var expectedView = new GameView()
            {
                Id = Guid.NewGuid(),
                Name = "Test Game",
            };

            var context = new GameViewContext(_contextOptions);
            await _connection.OpenAsync();
            context.Database.EnsureCreated();
            await context.AddAsync(firstView);
            await context.AddAsync(expectedView);

            await context.SaveChangesAsync();

            Assert.That(await _service.Get(expectedView.Id), Is.EqualTo(expectedView)
                .Using<GameView, GameView>((a, e) => a.Id == e.Id && a.Name == e.Name));
        }
    }
}