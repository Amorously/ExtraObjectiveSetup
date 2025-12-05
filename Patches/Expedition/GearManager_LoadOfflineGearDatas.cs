using Gear;
using ExtraObjectiveSetup.Expedition.Gears;
using HarmonyLib;

namespace ExtraObjectiveSetup.Patches.Expedition
{
    [HarmonyPatch]
    internal static class GearManager_LoadOfflineGearDatas
    {
        // called on both host and client side
        
        [HarmonyPatch(typeof(GearManager), nameof(GearManager.LoadOfflineGearDatas))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void Post_GearManager_LoadOfflineGearDatas(GearManager __instance)
        {
            ExpeditionGearManager.Current.VanillaGearManager = __instance;

            foreach (var gearSlot in ExpeditionGearManager.Current.GearSlots)
            {
                foreach (GearIDRange gearIDRange in __instance.m_gearPerSlot[(int)gearSlot.inventorySlot])
                {
                    uint playerOfflineDBPID = ExpeditionGearManager.GetOfflineGearPID(gearIDRange);
                    gearSlot.loadedGears.Add(playerOfflineDBPID, gearIDRange);
                }
            }
        }
    }
}
