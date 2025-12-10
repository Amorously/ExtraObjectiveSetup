using Gear;
using ExtraObjectiveSetup.Expedition.Gears;
using HarmonyLib;

namespace ExtraObjectiveSetup.Patches.Expedition
{
    [HarmonyPatch(typeof(GearManager), nameof(GearManager.LoadOfflineGearDatas))]
    internal static class GearManager_LoadOfflineGearDatas
    {
        // called on both host and client side
        
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
