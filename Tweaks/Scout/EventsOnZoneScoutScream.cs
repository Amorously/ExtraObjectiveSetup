using ExtraObjectiveSetup.BaseClasses;
using GameData;

namespace ExtraObjectiveSetup.Tweaks.Scout
{
    public class EventsOnZoneScoutScream: GlobalBased
    {
        public bool SuppressVanillaScoutWave { get; set; } = false;

        public List<WardenObjectiveEventData> EventsOnScoutScream { get; set; } = new();
    }
}
