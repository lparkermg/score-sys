using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ScoreSys.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreSys.Api.Tests
{
    public class ScoreSqlQueryServiceTest
    {
        private ScoreSqlQueryService _service;
        private DbContextOptions _contextOptions;
        private SqliteConnection _connection;
        private Guid _gameId1;
        private Guid _gameId2;

        private ScoreView[] _scores;

        [SetUp]
        public async Task SetUp()
        {
            _gameId1 = Guid.NewGuid();
            _gameId2 = Guid.NewGuid();

            _connection = new SqliteConnection("DataSource=:memory:");
            _contextOptions = new DbContextOptionsBuilder().UseSqlite(_connection).Options;
            _service = new ScoreSqlQueryService(_contextOptions);
            _scores = new[]
            {
                BuildScore(_gameId1, 5, "Test User"),
                BuildScore(_gameId2, 25, "Another User"),
                BuildScore(_gameId2, 100, "Test User"),
                BuildScore(_gameId1, 26, "Third User"),
                BuildScore(_gameId1, 50, "Forth User"),
            };

            await _connection.OpenAsync();

            using (var context = new ScoreViewContext(_contextOptions))
            {
                await context.Database.EnsureCreatedAsync();
                await context.Scores.AddRangeAsync(_scores);
                await context.SaveChangesAsync();
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            await _connection.CloseAsync();
        }

        [Test]
        public void Get_GivenEmptyGameId_ThrowsArgumentException()
            => Assert.That(async () => await _service.Get(Guid.Empty), Throws.ArgumentException.With.Message.EqualTo("Game Id cannot be empty"));

        [TestCase(0)]
        [TestCase(-5)]
        public void Get_GivenInvalidTakeAmount_ThrowsArgumentException(int take)
            => Assert.That(async () => await _service.Get(Guid.NewGuid(), take), Throws.ArgumentException.With.Message.EqualTo("Take amount must be above 0"));

        [Test]
        public void Get_GivenNegiativeSkipAmount_ThrowsArgumentException()
            => Assert.That(async () => await _service.Get(Guid.NewGuid(), skip: -5), Throws.ArgumentException.With.Message.EqualTo("Skip amnount must be 0 or above"));

        [Test]
        public async Task Get_GivenValidGameId_ReturnsExpectedTopScores()
        {
            var expectedScores = new List<ScoreView>()
            {
                _scores[0],
                _scores[3],
                _scores[4]
            }.OrderByDescending(s => s.Score);

            var scores = await _service.Get(_gameId1);

            Assert.That(scores, Is.EquivalentTo(expectedScores)
                .Using<ScoreView, ScoreView>((a, e) =>
                    a.Id == e.Id &&
                    a.GameId == e.GameId &&
                    a.Name == e.Name &&
                    a.Score == e.Score &&
                    a.PostedAt == e.PostedAt));

            Assert.That(scores, Is.Ordered.Descending.By("Score"));
        }

        [Test]
        public async Task Get_GivenValidGameIdWith2Take_ReturnsTop2Scores()
        {
            var expectedScores = new List<ScoreView>()
            {
                _scores[3],
                _scores[4]
            }.OrderByDescending(s => s.Score);

            var scores = await _service.Get(_gameId1, 2);

            Assert.That(scores, Is.EquivalentTo(expectedScores)
                .Using<ScoreView, ScoreView>((a, e) =>
                    a.Id == e.Id &&
                    a.GameId == e.GameId &&
                    a.Name == e.Name &&
                    a.Score == e.Score &&
                    a.PostedAt == e.PostedAt));

            Assert.That(scores, Is.Ordered.Descending.By("Score"));
        }

        [Test]
        public async Task Get_GivenValidGameIdWith2Skip_ReturnsExpectedScore()
        {
            var expectedScores = new List<ScoreView>()
            {
                _scores[0]
            };

            var scores = await _service.Get(_gameId1, skip: 2);

            Assert.That(scores, Is.EquivalentTo(expectedScores)
                .Using<ScoreView, ScoreView>((a, e) =>
                    a.Id == e.Id &&
                    a.GameId == e.GameId &&
                    a.Name == e.Name &&
                    a.Score == e.Score &&
                    a.PostedAt == e.PostedAt));
        }

        private ScoreView BuildScore(Guid gameId, int score, string name)
            => new ScoreView()
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Score = score,
                Name = name,
                PostedAt = DateTime.UtcNow,
            };
    }
}
