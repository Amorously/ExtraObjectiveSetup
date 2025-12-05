namespace ExtraObjectiveSetup.Tweaks.TerminalTweak
{
    public struct TerminalState
    {
        public bool Enabled = true;

        public TerminalState() { }

        public TerminalState(bool Enabled) { this.Enabled = Enabled; }

        public TerminalState(TerminalState o) { Enabled = o.Enabled; }
    }
}
