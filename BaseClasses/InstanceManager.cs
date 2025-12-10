using GameData;
using LevelGeneration;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class InstanceManager<T> : BaseManager where T : Il2CppSystem.Object
    {
        public const uint INVALID_INSTANCE_INDEX = uint.MaxValue;

        protected override string DEFINITION_NAME => string.Empty;
        
        protected Dictionary<(eDimensionIndex, LG_LayerType, eLocalZoneIndex), Dictionary<IntPtr, uint>> Instances2Index { get; set; } = new();
        protected Dictionary<(eDimensionIndex, LG_LayerType, eLocalZoneIndex), List<T>> Index2Instance { get; set; } = new();

        protected override void OnBuildStart() => OnLevelCleanup();

        protected override void OnLevelCleanup()
        {
            Index2Instance.Clear();
            Instances2Index.Clear();
        }

        public virtual void Init() { }

        public virtual uint Register((eDimensionIndex, LG_LayerType, eLocalZoneIndex) globalZoneIndex, T instance)
        {
            if (instance == null) return INVALID_INSTANCE_INDEX;
            Dictionary<IntPtr, uint> instancesInZone;
            List<T> instanceIndexInZone;
            if (!Instances2Index.ContainsKey(globalZoneIndex))
            {
                instancesInZone = new();
                instanceIndexInZone = new();
                Instances2Index[globalZoneIndex] = instancesInZone;
                Index2Instance[globalZoneIndex] = instanceIndexInZone;
            }
            else
            {
                instancesInZone = Instances2Index[globalZoneIndex];
                instanceIndexInZone = Index2Instance[globalZoneIndex];
            }

            if (instancesInZone.ContainsKey(instance.Pointer))
            {
                EOSLogger.Error($"InstanceManager<{typeof(T)}>: trying to register duplicate instance! Skipped....");
                return INVALID_INSTANCE_INDEX;
            }

            uint instanceIndex = (uint)instancesInZone.Count; // valid index starts from 0

            instancesInZone[instance.Pointer] = instanceIndex;
            instanceIndexInZone.Add(instance);

            return instanceIndex;
        }

        public virtual uint Register(T instance) => Register(GetGlobalZoneIndex(instance), instance);
        
        public abstract (eDimensionIndex dim, LG_LayerType layer, eLocalZoneIndex zone) GetGlobalZoneIndex(T instance);

        public uint GetZoneInstanceIndex(T instance)
        {
            var globalZoneIndex = GetGlobalZoneIndex(instance);

            if (!Instances2Index.ContainsKey(globalZoneIndex)) return INVALID_INSTANCE_INDEX;

            var zoneInstanceIndices = Instances2Index[globalZoneIndex];
            return zoneInstanceIndices.ContainsKey(instance.Pointer) ? zoneInstanceIndices[instance.Pointer] : INVALID_INSTANCE_INDEX;
        }

        public T GetInstance((eDimensionIndex dim, LG_LayerType layer, eLocalZoneIndex zone) globalZoneIndex, uint instanceIndex)
        {
            if (!Index2Instance.ContainsKey(globalZoneIndex)) return default!;

            var zoneInstanceIndices = Index2Instance[globalZoneIndex];

            return instanceIndex < zoneInstanceIndices.Count ? zoneInstanceIndices[(int)instanceIndex] : null!;
        }

        public T GetInstance(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex, uint instanceIndex) => GetInstance((dimensionIndex, layerType, localIndex), instanceIndex);

        public List<T> GetInstancesInZone((eDimensionIndex dim, LG_LayerType layer, eLocalZoneIndex zone) globalZoneIndex) => Index2Instance.ContainsKey(globalZoneIndex) ? Index2Instance[globalZoneIndex] : null!;

        public List<T> GetInstancesInZone(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex) => GetInstancesInZone((dimensionIndex, layerType, localIndex));

        public bool IsRegistered(T instance)
        {
            var globalZoneIndex = GetGlobalZoneIndex(instance);
            if (!Instances2Index.ContainsKey(globalZoneIndex)) return false;

            return Instances2Index[globalZoneIndex].ContainsKey(instance.Pointer);
        }

        public IEnumerable<(eDimensionIndex dim, LG_LayerType layer, eLocalZoneIndex zone)> RegisteredZones() => Index2Instance.Keys;
    }
}
