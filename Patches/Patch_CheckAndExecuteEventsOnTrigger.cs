using GameData;
using HarmonyLib;

namespace ExtraObjectiveSetup.Patches
{
    [HarmonyPatch]
    internal class Patch_CheckAndExecuteEventsOnTrigger
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.CheckAndExecuteEventsOnTrigger), new Type[] 
        {
            typeof(WardenObjectiveEventData),
            typeof(eWardenObjectiveEventTrigger),
            typeof(bool),
            typeof(float)
        })]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        private static bool Pre_CheckAndExecuteEventsOnTrigger(WardenObjectiveEventData eventToTrigger, eWardenObjectiveEventTrigger trigger, bool ignoreTrigger, float currentDuration)
        {
            if (eventToTrigger == null || !ignoreTrigger && eventToTrigger.Trigger != trigger || currentDuration != 0.0 && eventToTrigger.Delay <= currentDuration)
                return true;

            uint eventID = (uint)eventToTrigger.Type;
            if (!EOSWardenEventManager.Current.HasEventDefinition(eventID)) return true;

            string msg = EOSWardenEventManager.Current.IsVanillaEventID(eventID) ? "overriding vanilla event implementation..." : "executing...";
            EOSLogger.Debug($"WardenEvent: found definition for event ID {eventID}, {msg}");
            EOSWardenEventManager.Current.ExecuteEvent(eventToTrigger, currentDuration);
            return false;
        }
    }
}
