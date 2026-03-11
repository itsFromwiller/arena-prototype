namespace Arena.Tavern
{
    public enum GossipEffect
    {
        None,
        UnlockDungeon,
        Info,
    }

    public class GossipData
    {
        public string Name;
        public int MinLevel;
        public string GossipEffectName;
        public GossipEffect GossipEffect;  // Parsed at runtime, not from data
        public string EffectValue;
        public string GossipText;
    }
}