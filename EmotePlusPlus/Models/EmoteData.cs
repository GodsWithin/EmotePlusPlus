namespace EmotePlusPlus.Models
{
    public class EmoteData
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public int Uses { get; set; }
        public ulong ChannelId { get; set; }
        public bool Animated { get; set; }
    }
}
