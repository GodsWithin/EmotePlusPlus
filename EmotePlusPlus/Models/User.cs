using System.Collections.Generic;

namespace EmotePlusPlus.Models
{
    public class User
    {
        public ulong Id { get; set; }
        public List<EmoteData> UsedEmotes { get; set; }
    }
}
