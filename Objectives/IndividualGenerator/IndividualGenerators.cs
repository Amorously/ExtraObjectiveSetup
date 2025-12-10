using ExtraObjectiveSetup.BaseClasses;
using GameData;

namespace ExtraObjectiveSetup.Objectives.IndividualGenerator
{
    public class IndividualGeneratorDefinition : BaseInstanceDefinition
    {
        public bool ForceAllowPowerCellInsertion { get; set; } = false;

        public List<WardenObjectiveEventData> EventsOnInsertCell { get; set; } = new();

        public Vec3 Position { get; set; } = new();

        public Vec3 Rotation { get; set; } = new();
    }
}