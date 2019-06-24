using System;

namespace EmotePlusPlus.Models
{
    public class EmoteUse
    {
        public int Id { get; set; }
        public Emote Emote { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
