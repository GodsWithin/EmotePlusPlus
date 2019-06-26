using System;

namespace EmotePlusPlus.Models
{
    public class EmoteUse
    {
        public int Id { get; set; }
        public ulong EmoteId { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public string EmoteName { get; set; }
        public bool Animated { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
