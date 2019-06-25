namespace EmotePlusPlus.Models
{
    public class EmoteQueryResult
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public int Uses { get; set; }
        public bool Animated { get; set; }

        public override string ToString()
        {
            return $"<{(Animated ? "a" : "")}:{Name}:{Id}> {string.Format("{0:n0}", Uses)}";
        }
    }
}
