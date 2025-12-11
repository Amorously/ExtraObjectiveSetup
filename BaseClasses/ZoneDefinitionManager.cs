using AmorLib.Utils;
using ExtraObjectiveSetup.JSON;
using GameData;
using GTFO.API.Utilities;
using LevelGeneration;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class ZoneDefinitionManager<T> : BaseManager where T : GlobalBased, new()
    {
        protected Dictionary<uint, ZoneDefinitionsForLevel<T>> Definitions { get; set; } = new();
        protected Dictionary<uint, ZoneDefinitionsForLevel<T>> definitions { get => Definitions; set => Definitions = value; }

        protected override void ReadFiles()
        {
            File.WriteAllText(Path.Combine(DEFINITION_PATH, "Template.json"), EOSJson.Serialize(new ZoneDefinitionsForLevel<T>()));

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                var conf = EOSJson.Deserialize<ZoneDefinitionsForLevel<T>>(content);
                AddDefinitions(conf);
            }
        }

        protected override void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                ZoneDefinitionsForLevel<T> conf = EOSJson.Deserialize<ZoneDefinitionsForLevel<T>>(content);
                AddDefinitions(conf);
            });
        }

        protected virtual void AddDefinitions(ZoneDefinitionsForLevel<T> definitions)
        {
            if (definitions == null) return;

            if (Definitions.ContainsKey(definitions.MainLevelLayout))
            {
                EOSLogger.Log("Replaced MainLevelLayout {0}", definitions.MainLevelLayout);
            }

            Definitions[definitions.MainLevelLayout] = definitions;
        }

        public virtual List<T> GetDefinitionsForLevel(uint MainLevelLayout) => Definitions.ContainsKey(MainLevelLayout) ? Definitions[MainLevelLayout].Definitions : null!;

        public virtual T GetDefinition((eDimensionIndex, LG_LayerType, eLocalZoneIndex) globalIndex) => GetDefinition(globalIndex.Item1, globalIndex.Item2, globalIndex.Item3);

        public virtual T GetDefinition(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex)
        {
            if (!Definitions.ContainsKey(CurrentMainLevelLayout)) return null!;
            var tuple = GlobalIndexUtil.ToIntTuple(dimensionIndex, layerType, localIndex);
            return Definitions[CurrentMainLevelLayout].Definitions.Find(def => def.IntTuple == tuple)!;
        }
        
        protected void Sort(ZoneDefinitionsForLevel<T> levelDefs)
        {
            levelDefs.Definitions.Sort((u1, u2) => u1.IntTuple.CompareTo(u2.IntTuple));
        }
    }
}
