using ExtraObjectiveSetup.BaseClasses;
using GameData;

namespace ExtraObjectiveSetup.Objectives.ObjectiveCounter
{
    public sealed class ObjectiveCounterManager : GenericExpeditionDefinitionManager<ObjectiveCounterDefinition>
    {
        protected override string DEFINITION_NAME => "ObjectiveCounter";
        
        public static ObjectiveCounterManager Current { get; } = new();

        private Dictionary<string, Counter> _counters { get; } = new();        

        public ObjectiveCounterManager() 
        {
            EOSWardenEventManager.Current.AddEventDefinition(CounterWardenEvent.ChangeCounter.ToString(), (uint)CounterWardenEvent.ChangeCounter, ChangeCounter);               
        }
        
        protected override void OnBuildStart() => Clear();
        protected override void OnBuildDone() => BuildCounters();   
        protected override void OnLevelCleanup() => Clear();

        private void BuildCounters()
        {
            if (!Definitions.ContainsKey(CurrentMainLevelLayout)) return;
            Definitions[CurrentMainLevelLayout].Definitions.ForEach(Build);
        }

        private void Clear()
        {
            _counters.Clear();
        }
        
        private void Build(ObjectiveCounterDefinition def)
        {
            if(_counters.ContainsKey(def.WorldEventObjectFilter))
            {
                EOSLogger.Error($"Build Counter: counter '{def.WorldEventObjectFilter}' already exists...");
                return;
            }

            var counter = new Counter(def);
            if(!counter.TrySetupReplicator())
            {
                EOSLogger.Error($"Build Counter: counter '{def.WorldEventObjectFilter}' failed to setup state replicator! What's going wrong?");
                return;
            }

            _counters[def.WorldEventObjectFilter] = counter;
            EOSLogger.Debug($"Build Counter: counter '{def.WorldEventObjectFilter}' setup completed");
        }

        public void ChangeCounter(string worldEventObjectFilter, int by)
        {
            if(!_counters.ContainsKey(worldEventObjectFilter))
            {
                EOSLogger.Error($"ChangeCounter: {worldEventObjectFilter} is not defined");
                return;
            }

            if (by == 0) return;

            var counter = _counters[worldEventObjectFilter];
            if(by > 0)
            {
                counter.Increment(by);
            }
            else
            {
                counter.Decrement(-by);
            }
        }

        private void ChangeCounter(WardenObjectiveEventData e)
        {
            string worldEventObjectFilter = e.WorldEventObjectFilter;
            int by = e.Count;
            ChangeCounter(worldEventObjectFilter, by);
        }
    }
}
