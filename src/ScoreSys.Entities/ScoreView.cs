using System;

namespace ScoreSys.Entities
{
    public sealed class ScoreView
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }

        public string Name { get; set; }

        public int Score { get; set; }

        public DateTime PostedAt { get; set; }
    }
}
