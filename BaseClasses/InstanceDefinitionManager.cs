using AmorLib.Utils;
using ExtraObjectiveSetup.JSON;
using GameData;
using GTFO.API.Utilities;
using LevelGeneration;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class InstanceDefinitionManager<T> : BaseManager where T : BaseInstanceDefinition, new()
    { 
        protected Dictionary<uint, InstanceDefinitionsForLevel<T>> Definitions { get; set; } = new();
        protected Dictionary<uint, InstanceDefinitionsForLevel<T>> definitions { get => Definitions; set => Definitions = value; }

        protected override void ReadFiles()
        {
            File.WriteAllText(Path.Combine(DEFINITION_PATH, "Template.json"), EOSJson.Serialize(new InstanceDefinitionsForLevel<T>()));

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                var conf = EOSJson.Deserialize<InstanceDefinitionsForLevel<T>>(content);
                AddDefinitions(conf);
            }
        }

        protected virtual void AddDefinitions(InstanceDefinitionsForLevel<T> definitions)
        {
            if (definitions == null) return;

            if (Definitions.ContainsKey(definitions.MainLevelLayout))
            {
                EOSLogger.Log("Replaced MainLevelLayout {0}", definitions.MainLevelLayout);
            }

            Definitions[definitions.MainLevelLayout] = definitions;
        }

        protected override void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                InstanceDefinitionsForLevel<T> conf = EOSJson.Deserialize<InstanceDefinitionsForLevel<T>>(content);
                AddDefinitions(conf);
            });
        }
        
        public virtual List<T> GetDefinitionsForLevel(uint MainLevelLayout) => Definitions.ContainsKey(MainLevelLayout) ? Definitions[MainLevelLayout].Definitions : null!;

        public T GetDefinition((eDimensionIndex, LG_LayerType, eLocalZoneIndex) globalIndex, uint instanceIndex) => GetDefinition(globalIndex.Item1, globalIndex.Item2, globalIndex.Item3, instanceIndex);

        public virtual T GetDefinition(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex, uint instanceIndex)
        {
            if (!Definitions.ContainsKey(CurrentMainLevelLayout)) return null!;
            var tuple = GlobalIndexUtil.ToIntTuple(dimensionIndex, layerType, localIndex);
            return Definitions[CurrentMainLevelLayout].Definitions.Find(def => def.IntTuple == tuple)!;
        }

        protected void Sort(InstanceDefinitionsForLevel<T> levelDefs)
        {
            levelDefs.Definitions.Sort((u1, u2) =>
            {
                int cmp = u1.IntTuple.CompareTo(u2.IntTuple);
                if (cmp != 0) return cmp;
                return u1.InstanceIndex.CompareTo(u2.InstanceIndex);
            });
        }        
    }
}
