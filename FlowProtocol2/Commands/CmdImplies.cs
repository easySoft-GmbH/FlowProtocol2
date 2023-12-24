namespace FlowProtocol2.Commands
{
    using System.Text.RegularExpressions;
    using FlowProtocol2.Core;

    /// <summary>
    /// Implementiert den Implies-Befehl
    /// </summary>
    public class CmdImplies : CmdBaseCommand
    {
        public string VarName { get; set; }
        public string Text { get; set; }

        public static CommandParser GetComandParser()
        {
            return new CommandParser(@"^~Implies ([A-Za-z0-9$]*)\s*=(.*)", (rc, m) => CreateImpliesCommand(rc, m));
        }

        private static CmdBaseCommand CreateImpliesCommand(ReadContext rc, Match m)
        {
            CmdImplies cmd = new CmdImplies(rc);
            cmd.VarName = m.Groups[1].Value.Trim();
            cmd.Text = m.Groups[2].Value.Trim();
            return cmd;
        }

        public CmdImplies(ReadContext readcontext) : base(readcontext)
        {
            VarName = string.Empty;
            Text = string.Empty;
        }

        public override CmdBaseCommand? Run(RunContext rc)
        {
            string varNameExpanded = ReplaceVars(rc, VarName).Trim();
            rc.BoundVars[varNameExpanded] = ReplaceVars(rc, Text);
            rc.GivenKeys.Add(varNameExpanded);
            return NextCommand;
            //passt noch nicht
        }
    }
}