using System;

namespace ScoreSys.Entities
{
    public class GameView
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
            => $"{Id}|{Name}";

        public void FromString(string data)
        {
            var split = data.Split("|");
            Id = Guid.Parse(split[0]);
            Name = split[1];
        }
    }
}
