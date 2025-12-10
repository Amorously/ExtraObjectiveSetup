using ChainedPuzzles;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Instances.ChainedPuzzle;
using GameData;

namespace ExtraObjectiveSetup.Objectives.GeneratorCluster
{
    internal sealed class GeneratorClusterObjectiveManager : InstanceDefinitionManager<GeneratorClusterDefinition>
    {
        protected override string DEFINITION_NAME { get; } = "GeneratorCluster";
        
        public static GeneratorClusterObjectiveManager Current { get; private set; } = new();        

        private List<(LG_PowerGeneratorCluster, GeneratorClusterDefinition)> _chainedPuzzleToBuild = new();

        protected override void OnBuildStart() => OnLevelCleanup(); 

        protected override void OnBuildDone()
        {
            BuildChainedPuzzleMidObjective();
        }

        protected override void OnLevelCleanup()
        {
            _chainedPuzzleToBuild.Clear();
        }
        
        protected override void AddDefinitions(InstanceDefinitionsForLevel<GeneratorClusterDefinition> definitions)
        {
            // because we have chained puzzles, sorting is necessary to preserve chained puzzle instance order.
            Sort(definitions);
            base.AddDefinitions(definitions);
        }

        internal void RegisterForChainedPuzzleBuild(LG_PowerGeneratorCluster __instance, GeneratorClusterDefinition GeneratorClusterConfig) => _chainedPuzzleToBuild.Add((__instance, GeneratorClusterConfig));

        private void BuildChainedPuzzleMidObjective()
        {
            foreach (var tuple in _chainedPuzzleToBuild)
            {
                LG_PowerGeneratorCluster __instance = tuple.Item1;
                var config = tuple.Item2;
                uint persistentId = config.EndSequenceChainedPuzzle;

                var block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(persistentId);

                if (block != null)
                {
                    EOSLogger.Debug($"GeneratorCluster: Building EndSequenceChainedPuzzle for LG_PowerGeneratorCluster in {__instance.SpawnNode.m_zone.LocalIndex}, {__instance.SpawnNode.LayerType}, {__instance.SpawnNode.m_dimension.DimensionIndex}");

                    __instance.m_chainedPuzzleMidObjective = ChainedPuzzleManager.CreatePuzzleInstance(
                        block,
                        __instance.SpawnNode.m_area,
                        __instance.m_chainedPuzzleAlignMidObjective.position,
                        __instance.m_chainedPuzzleAlignMidObjective);

                    __instance.m_chainedPuzzleMidObjective.Add_OnStateChange((_, newState, isRecall) =>
                    {
                        switch(newState.status)
                        {
                            case eChainedPuzzleStatus.Solved:
                                if (!isRecall)
                                {
                                    EOSWardenEventManager.ExecuteWardenEvents(config.EventsOnEndSequenceChainedPuzzleComplete);
                                }
                                break;
                        }
                    });
                }
            }
        }
    }
}
