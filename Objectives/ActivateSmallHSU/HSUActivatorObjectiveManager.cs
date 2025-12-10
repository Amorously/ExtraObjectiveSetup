using ChainedPuzzles;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Instances.ChainedPuzzle;
using GameData;
using UnityEngine;

namespace ExtraObjectiveSetup.Objectives.ActivateSmallHSU
{
    internal sealed class HSUActivatorObjectiveManager : InstanceDefinitionManager<HSUActivatorDefinition>
    {
        protected override string DEFINITION_NAME { get; } = "ActivateSmallHSU";
        
        public static HSUActivatorObjectiveManager Current { get; private set; } = new();   
        
        private readonly Dictionary<IntPtr, HSUActivatorDefinition> _hsuActivatorPuzzles = new(); // key: ChainedPuzzleInstance.Pointer    

        protected override void OnBuildStart() => OnLevelCleanup();

        protected override void OnBuildDone()
        {
            if (!Definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            Definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(BuildHSUActivatorChainedPuzzle);
        }

        protected override void OnLevelCleanup()
        {
            //if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            //definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(def => { def.ChainedPuzzleOnActivationInstance = null; });
            foreach (var h in _hsuActivatorPuzzles.Values)
            {
                h.ChainedPuzzleOnActivationInstance = null!;
            }

            _hsuActivatorPuzzles.Clear();
        }

        protected override void AddDefinitions(InstanceDefinitionsForLevel<HSUActivatorDefinition> definitions)
        {
            // because we have chained puzzles, sorting is necessary to preserve chained puzzle instance order.
            Sort(definitions);
            base.AddDefinitions(definitions);
        }

        private void BuildHSUActivatorChainedPuzzle(HSUActivatorDefinition def)
        {
            var instance = HSUActivatorInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);
            if (instance == null)
            {
                EOSLogger.Error($"Found unused HSUActivator config: {(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex)}");
                return;
            }

            if (def.RequireItemAfterActivationInExitScan == true)
            {
                instance.m_sequencerExtractionDone.OnSequenceDone += new Action(() => {
                    WardenObjectiveManager.AddObjectiveItemAsRequiredForExitScan(true, new iWardenObjectiveItem[1] { new iWardenObjectiveItem(instance.m_linkedItemComingOut.Pointer) });
                    EOSLogger.Debug($"HSUActivator: {(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex)} - added required item for extraction scan");
                });
            }

            if (def.TakeOutItemAfterActivation)
            {
                instance.m_sequencerExtractionDone.OnSequenceDone += new Action(() => {
                    instance.LinkedItemComingOut.m_navMarkerPlacer.SetMarkerVisible(true);
                });
            }

            if (def.ChainedPuzzleOnActivation != 0)
            {
                var block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(def.ChainedPuzzleOnActivation);
                if (block == null)
                {
                    EOSLogger.Error($"HSUActivator: ChainedPuzzleOnActivation is specified but ChainedPuzzleDatablock definition is not found, won't build");
                }
                else
                {
                    Vector3 startPosition = def.ChainedPuzzleStartPosition.ToVector3();

                    if (startPosition == Vector3.zeroVector)
                    {
                        startPosition = instance.m_itemGoingInAlign.position;
                    }

                    var puzzleInstance = ChainedPuzzleManager.CreatePuzzleInstance(
                        block,
                        instance.SpawnNode.m_area,
                        startPosition,
                        instance.SpawnNode.m_area.transform);

                    def.ChainedPuzzleOnActivationInstance = puzzleInstance;

                    _hsuActivatorPuzzles[puzzleInstance.Pointer] = def;

                    // PuzzleInstance will be activated in SyncStateChanged
                    // EventsOnActivationScanSolved and HSU removeSequence will be executed in 
                    // ChainedPuzzleInstance.OnStateChanged(patch ChainedPuzzleInstance_OnPuzzleSolved)

                    puzzleInstance.Add_OnStateChange((_, newState, isRecall) =>
                    {
                        switch (newState.status) 
                        {
                            case eChainedPuzzleStatus.Solved:
                                if (!isRecall)
                                {
                                    EOSWardenEventManager.ExecuteWardenEvents(def.EventsOnActivationScanSolved);
                                    if (def.TakeOutItemAfterActivation)
                                    {
                                        instance.m_triggerExtractSequenceRoutine = instance.StartCoroutine(instance.TriggerRemoveSequence());
                                    }
                                }
                                break;
                        }
                    });

                    EOSLogger.Debug($"HSUActivator: ChainedPuzzleOnActivation ID: {def.ChainedPuzzleOnActivation} specified and created");
                }
            }
            else
            {
                if (def.TakeOutItemAfterActivation)
                {
                    instance.m_triggerExtractSequenceRoutine = instance.StartCoroutine(instance.TriggerRemoveSequence());
                }
            }
        }

        internal HSUActivatorDefinition GetHSUActivatorDefinition(ChainedPuzzleInstance chainedPuzzle) => _hsuActivatorPuzzles.TryGetValue(chainedPuzzle.Pointer, out var def) ? def : null!;
    }
}