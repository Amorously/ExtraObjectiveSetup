using AmorLib.Networking.StateReplicators;
using LevelGeneration;
using Player;
using SNetwork;

namespace ExtraObjectiveSetup.Tweaks.TerminalTweak
{
    public class TerminalWrapper
    {
        public LG_ComputerTerminal lgTerminal { get; private set; }

        public StateReplicator<TerminalState> stateReplicator { get; private set; }

        private void ChangeStateUnsynced(bool enabled)
        {
            //EOSLogger.Debug($"{lgTerminal.ItemKey} state, enabled: {enabled}");

            lgTerminal.OnProximityExit();
            Interact_ComputerTerminal interact = lgTerminal.GetComponentInChildren<Interact_ComputerTerminal>(true);
            bool active = enabled;

            if (interact != null)
            {
                interact.enabled = active;
                interact.SetActive(active);
            }

            lgTerminal.m_interfaceScreen.SetActive(active);
            lgTerminal.m_loginScreen.SetActive(active);

            if (lgTerminal.m_text != null)
            {
                lgTerminal.m_text.enabled = active;
            }

            if (!active)
            {
                PlayerAgent interactionSource = lgTerminal.m_localInteractionSource;
                if (interactionSource != null && interactionSource.FPItemHolder.InTerminalTrigger)
                {
                    lgTerminal.ExitFPSView();
                }
            }

        }

        public void ChangeState(bool enabled)
        {
            ChangeStateUnsynced(enabled);
            if (SNet.IsMaster)
            {
                stateReplicator.SetState(new() { Enabled = enabled });
            }
        }

        private void OnStateChanged(TerminalState oldState, TerminalState newState, bool isRecall)
        {
            if (!isRecall) return;

            ChangeStateUnsynced(newState.Enabled);
        }

        public static TerminalWrapper Instantiate(LG_ComputerTerminal lgTerminal, uint replicatorID)
        {
            if (lgTerminal == null || replicatorID == EOSNetworking.INVALID_ID) return null!;
            var t = new TerminalWrapper();

            t.lgTerminal = lgTerminal;
            t.stateReplicator = StateReplicator<TerminalState>.Create(replicatorID, new() { Enabled = true }, LifeTimeType.Session)!;
            t.stateReplicator.OnStateChanged += t.OnStateChanged;

            return t;
        }
    }
}
