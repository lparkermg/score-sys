using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ScoreSys.Entities
{
    public class ScoreView
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }

        public string Name { get; set; }

        public int Score { get; set; }

        public DateTime PostedAt { get; set; }

        public override string ToString()
            => $"{Id}|{GameId}|{Name}|{Score}|{PostedAt}";

        // TODO: Work out a better way of doing this, or just better serialisation.
        public void FromString(string data)
        {
            var split = data.Split('|');
            Id = Guid.Parse(split[0]);
            GameId = Guid.Parse(split[1]);
            Name = split[2];
            Score = int.Parse(split[3]);
            PostedAt = DateTime.Parse(split[4]);
        }
    }
}
