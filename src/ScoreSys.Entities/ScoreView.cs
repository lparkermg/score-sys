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

    // TODO: Wrap in tests.
    public static class ScoreViewExtenstions
    {
        public static byte[] ToBytes(this ScoreView view)
        {

            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, view.ToString());
                return ms.ToArray();
            }
        }

        public static ScoreView BytesToScoreView(byte[] data)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream) as string;
                var view = new ScoreView();
                view.FromString(obj);
                return view;
            }
        }
    }
}
