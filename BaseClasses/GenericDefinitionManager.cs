using ExtraObjectiveSetup.JSON;
using GTFO.API.Utilities;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class GenericDefinitionManager<T> : BaseManager where T: new()
    {
        protected Dictionary<uint, GenericDefinition<T>> Definitions { get; set; } = new();

        protected override void ReadFiles()
        {
            File.WriteAllText(Path.Combine(DEFINITION_PATH, "Template.json"), EOSJson.Serialize(new GenericDefinition<T>()));

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                var conf = EOSJson.Deserialize<GenericDefinition<T>>(content);
                AddDefinitions(conf);
            }
        }

        protected override void OnFileChanged(LiveEditEventArgs e) => FileChanged(e);

        protected virtual void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                var conf = EOSJson.Deserialize<GenericDefinition<T>>(content);
                AddDefinitions(conf);
            });
        }
        
        protected virtual void AddDefinitions(GenericDefinition<T> definition)
        {
            if (definition == null) return;

            if (Definitions.ContainsKey(definition.ID))
            {
                EOSLogger.Log("Replaced ID {0}", definition.ID);
            }

            Definitions[definition.ID] = definition;
        }
        
        public GenericDefinition<T> GetDefinition(uint ID) => Definitions.ContainsKey(ID) ? Definitions[ID] : null!;
    }
}
