namespace FlowProtocol2.Commands
{
    using System.Text.RegularExpressions;
    using FlowProtocol2.Core;

    /// <summary>
    /// Implementiert den ToLower-Befehl
    /// </summary>
    public class CmdToLower : CmdBaseCommand
    {
        public string VarName { get; set; }
        public string Text { get; set; }

        public static CommandParser GetComandParser()
        {
            return new CommandParser(@"^~ToLower ([A-Za-z0-9\$\(\)]*)\s*=(.*)", (rc, m) => CreateToLowerCommand(rc, m));
        }

        private static CmdBaseCommand CreateToLowerCommand(ReadContext rc, Match m)
        {
            CmdToLower cmd = new CmdToLower(rc);
            cmd.VarName = m.Groups[1].Value.Trim();
            cmd.Text = m.Groups[2].Value.Trim();
            return cmd;
        }

        public CmdToLower(ReadContext readcontext) : base(readcontext)
        {
            VarName = string.Empty;
            Text = string.Empty;
        }

        public override CmdBaseCommand? Run(RunContext rc)
        {
            string expandedVarName = ReplaceVars(rc, VarName);
            string expandedText = ReplaceVars(rc, Text);
            try
            {
                rc.InternalVars[expandedVarName] = expandedText.ToLower();
            }
            catch (Exception ex)
            {
                rc.SetError(ReadContext, "Verarbeitungfehler",
                    $"Beim Ausführen des Skriptes ist ein Fehler aufgetreten '{ex.Message}'. Dies Ausführung wird abgebrochen."
                    + $"Variablenwerte: expandedVarName='{expandedVarName}' expandedText='{expandedText}'");
                return null;
            }
            return NextCommand;
        }
    }
}