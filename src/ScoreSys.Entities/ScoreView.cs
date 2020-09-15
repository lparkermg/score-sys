using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

    public static class ScoreViewExtenstions
    {
        public static byte[] ToBytes(this ScoreView view)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, view);
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
                var obj = binForm.Deserialize(memStream) as ScoreView;
                return obj;
            }
        }
    }
}
