using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Objectives.IndividualGenerator
{
    internal sealed class IndividualGeneratorObjectiveManager : InstanceDefinitionManager<IndividualGeneratorDefinition>
    {
        protected override string DEFINITION_NAME { get; } = "IndividualGenerator";
        
        public static IndividualGeneratorObjectiveManager Current { private set; get; } = new();        
    }
}
