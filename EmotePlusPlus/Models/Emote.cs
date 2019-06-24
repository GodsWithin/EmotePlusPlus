using System;

namespace EmotePlusPlus.Models
{
    public class Emote
    {
        public string Name { get; set; }
        public ulong Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Uses { get; set; }
        public bool Animated { get; set; }

        public override string ToString()
        {
            return $"<:{Name}:{Id}> has been used {Uses} time{(Uses == 1 ? "" : "s")} in this server.";
        }
    }
}
