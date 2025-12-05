using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Objectives.IndividualGenerator;
using ExtraObjectiveSetup.Utils;
using GameData;
using HarmonyLib;
using LevelGeneration;
using Player;
using SNetwork;

namespace ExtraObjectiveSetup.Patches.PowerGenerator
{
    [HarmonyPatch]
    internal static class Patch_LG_PowerGenerator_Core_Setup
    {
        [HarmonyPatch(typeof(LG_PowerGenerator_Core), nameof(LG_PowerGenerator_Core.Setup))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void Post_PowerGenerator_Setup(LG_PowerGenerator_Core __instance)
        {
            // do some minor vanilla bug fix
            __instance.m_powerCellInteraction.AttemptCarryItemInsert += new System.Action<SNet_Player, Item>((p, item) =>
            {

                if (PlayerBackpackManager.TryGetItemInLevelFromItemData(item.Get_pItemData(), out var itemInLevel))
                {
                    var cell = itemInLevel.Cast<ItemInLevel>();
                    cell.CanWarp = false;
                }
                else
                {
                    EOSLogger.Error($"Inserting sth other than PowerCell ({item.PublicName}) into {__instance.m_itemKey}, how?");
                }
            });

            if (PowerGeneratorInstanceManager.Current.IsGCGenerator(__instance)) return;

            uint zoneInstanceIndex = PowerGeneratorInstanceManager.Current.Register(__instance);
            var globalZoneIndex = PowerGeneratorInstanceManager.Current.GetGlobalZoneIndex(__instance);
            var def = IndividualGeneratorObjectiveManager.Current.GetDefinition(globalZoneIndex, zoneInstanceIndex);

            if (def == null) return;

            var position = def.Position.ToVector3();
            var rotation = def.Rotation.ToQuaternion();

            if (position != UnityEngine.Vector3.zero)
            {
                __instance.transform.position = position;
                __instance.transform.rotation = rotation;

                __instance.m_sound.UpdatePosition(position);

                EOSLogger.Debug($"LG_PowerGenerator_Core: modified position / rotation");
            }

            if (def.ForceAllowPowerCellInsertion)
            {
                __instance.SetCanTakePowerCell(true);
            }

            EOSLogger.Debug($"LG_PowerGenerator_Core: overriden, instance {zoneInstanceIndex} in {globalZoneIndex}");
        }
        
        [HarmonyPatch(typeof(LG_PowerGenerator_Core), nameof(LG_PowerGenerator_Core.SyncStatusChanged))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void Post_SyncStatusChanged(LG_PowerGenerator_Core __instance, pPowerGeneratorState state, bool isDropinState)
        {
            var zoneInstanceIndex = PowerGeneratorInstanceManager.Current.GetZoneInstanceIndex(__instance);
            var globalZoneIndex = PowerGeneratorInstanceManager.Current.GetGlobalZoneIndex(__instance);
            var def = IndividualGeneratorObjectiveManager.Current.GetDefinition(globalZoneIndex, zoneInstanceIndex);

            if (def == null || def.EventsOnInsertCell == null || state.status != ePowerGeneratorStatus.Powered || isDropinState) return;

            if (def.EventsOnInsertCell.Count > 0)
            {
                def.EventsOnInsertCell.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
            }
        }
    }
}
