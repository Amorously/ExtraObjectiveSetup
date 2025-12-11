global using ExtraObjectiveSetup.ExtendedWardenEvents;
global using ExtraObjectiveSetup.Utils;
using AmorLib.Dependencies;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.BaseClasses;
using GTFO.API;
using HarmonyLib;
using System.Reflection;

namespace ExtraObjectiveSetup
{
    [BepInPlugin(AUTHOR + "." + PLUGIN_NAME, PLUGIN_NAME, VERSION)]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.dak.MTFO", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Amor.AmorLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(InjectLib_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PData_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.sinai.UnityExplorer", BepInDependency.DependencyFlags.SoftDependency)]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string AUTHOR = "Inas";
        public const string PLUGIN_NAME = "ExtraObjectiveSetup";
        public const string VERSION = "1.7.0";

        private readonly List<Type[]> _callbackAssemblyTypes = new() { AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()) };

        public override void Load()
        { 
            new Harmony("ExtraObjectiveSetup").PatchAll();

            InteropAPI.RegisterCall("EOS_Managers", args =>
            {
                if (args?.Length > 0 && args[0] is Type[] types)
                {
                    _callbackAssemblyTypes.Add(types);
                }
                return null;
            });

            AssetAPI.OnStartupAssetsLoaded += SetupManagers;
            EOSLogger.Log("EOS is done loading!");
        }

        private void SetupManagers()
        {
            var managers = _callbackAssemblyTypes
                .SelectMany(types => types)
                .Where(t => typeof(BaseManager).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (BaseManager)Activator.CreateInstance(t, true)!);
            BaseManager.SetupManagers(managers);
        }
    }
}
