using ExtraObjectiveSetup.JSON;
using GTFO.API.Utilities;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class GenericExpeditionDefinitionManager<T> : BaseManager where T: new()
    {
        protected Dictionary<uint, GenericExpeditionDefinition<T>> Definitions { get; set; } = new();
        protected Dictionary<uint, GenericExpeditionDefinition<T>> definitions { get => Definitions; set => Definitions = value; }

        protected override void ReadFiles()
        {

            File.WriteAllText(Path.Combine(DEFINITION_PATH, "Template.json"), EOSJson.Serialize(new GenericExpeditionDefinition<T>()));

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                var conf = EOSJson.Deserialize<GenericExpeditionDefinition<T>>(content);
                AddDefinitions(conf);
            }
        }

        protected override void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                GenericExpeditionDefinition<T> conf = EOSJson.Deserialize<GenericExpeditionDefinition<T>>(content);
                AddDefinitions(conf);
            });
        }
        
        protected virtual void AddDefinitions(GenericExpeditionDefinition<T> definitions)
        {
            if (definitions == null) return;

            if (Definitions.ContainsKey(definitions.MainLevelLayout))
            {
                EOSLogger.Log("Replaced MainLevelLayout {0}", definitions.MainLevelLayout);
            }

            Definitions[definitions.MainLevelLayout] = definitions;
        }   
        
        public GenericExpeditionDefinition<T> GetDefinition(uint MainLevelLayout) => Definitions.ContainsKey(MainLevelLayout) ? Definitions[MainLevelLayout] : null!;        
    }
}
