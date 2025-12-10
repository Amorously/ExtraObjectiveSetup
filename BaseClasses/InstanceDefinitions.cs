using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.BaseClasses
{
    public class BaseInstanceDefinition: GlobalBased
    {
        [JsonPropertyOrder(-8)]
        public uint InstanceIndex { get; set; } = uint.MaxValue;        

        public override string ToString() => base.ToString() + $", Instance_{InstanceIndex}";
    }

    public class InstanceDefinitionsForLevel<T> where T : BaseInstanceDefinition, new()
    {
        public uint MainLevelLayout { set; get; } = 0;

        public List<T> Definitions { set; get; } = new() { new() };
    }
}
