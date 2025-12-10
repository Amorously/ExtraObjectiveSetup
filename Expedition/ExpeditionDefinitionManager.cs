using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.JSON;
using GTFO.API.Utilities;

namespace ExtraObjectiveSetup.Expedition
{
    public sealed class ExpeditionDefinitionManager : BaseManager
    {
        protected override string DEFINITION_NAME => "ExtraExpeditionSettings";
        
        public static ExpeditionDefinitionManager Current { get; private set; } = new();
        
        private readonly Dictionary<uint, ExpeditionDefinition> Definitions = new();

        protected override void ReadFiles()
        {
            File.WriteAllText(Path.Combine(DEFINITION_PATH, "Template.json"), EOSJson.Serialize(new ExpeditionDefinition()));

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                var conf = EOSJson.Deserialize<ExpeditionDefinition>(content);
                AddDefinitions(conf);
            }
        }

        protected override void OnFileChanged(LiveEditEventArgs e) 
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                var conf = EOSJson.Deserialize<ExpeditionDefinition>(content);
                AddDefinitions(conf);
            });
        }

        private void AddDefinitions(ExpeditionDefinition definitions)
        {
            if (definitions == null) return;

            if (Definitions.ContainsKey(definitions.MainLevelLayout))
            {
                EOSLogger.Log("Replaced MainLevelLayout {0}", definitions.MainLevelLayout);
            }

            Definitions[definitions.MainLevelLayout] = definitions;
        }

        public ExpeditionDefinition GetDefinition(uint MainLevelLayout) => Definitions.ContainsKey(MainLevelLayout) ? Definitions[MainLevelLayout] : null!;
    }
}
