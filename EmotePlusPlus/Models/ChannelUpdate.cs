using LiteDB;
using System;

namespace EmotePlusPlus.Models
{
    public class ChannelUpdate
    {
        [BsonId]
        public ulong ChannelId { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
