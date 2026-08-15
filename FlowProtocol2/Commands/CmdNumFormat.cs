namespace FlowProtocol2.Commands
{
    using System.Globalization;
    using System.Text.RegularExpressions;
    using FlowProtocol2.Core;

    /// <summary>
    /// Implementiert den NumFormat-Befehl
    /// </summary>
    /// <remarks>
    /// Erstellt mit NewCC.fp2, Eingabe: ~NumFormat (vVarName) = (fNumber) | (xFormat)
    /// </remarks>
    public class CmdNumFormat : CmdBaseCommand
    {
        public string VarName { get; set; }
        public string Number { get; set; }
        public string Format { get; set; }

        public static CommandParser GetComandParser()
        {
            return new CommandParser(@"^~NumFormat\s+([A-Za-z0-9\$\(\)]+)\s*=\s*(-?[A-Za-z0-9\$\(\)\.\,]+)\s*\|\s*(.+)",
                                     (rc, m) => CreateNumFormatCommand(rc, m));
        }

        private static CmdBaseCommand CreateNumFormatCommand(ReadContext rc, Match m)
        {
            CmdNumFormat cmd = new CmdNumFormat(rc);
            cmd.VarName = m.Groups[1].Value.Trim();
            cmd.Number = m.Groups[2].Value.Trim();
            cmd.Format = m.Groups[3].Value.Trim();
            return cmd;
        }

        public CmdNumFormat(ReadContext readcontext) : base(readcontext)
        {
            VarName = string.Empty;
            Number = string.Empty;
            Format = string.Empty;
        }

        public override CmdBaseCommand? Run(RunContext rc)
        {
            string expandedVarName = ReplaceVars(rc, VarName);
            string expandedNumber = ReplaceVars(rc, Number);
            string expandedFormat = ReplaceVars(rc, Format);
            try
            {
                bool bOKNumber = double.TryParse(expandedNumber, out double resultNumber);
                if (!bOKNumber)
                {
                    rc.SetError(ReadContext, "Ungültiger numerischer Ausdruck",
                            $"Der Ausdruck '{expandedNumber}' kann nicht als Gleitkommazahl interpretiert werden. Die Ausführung wird abgebrochen.");
                    return null;
                }
                // Ergebnis der Variablen zuweisen
                rc.InternalVars[expandedVarName] = resultNumber.ToString(expandedFormat, rc.Culture);
            }
            catch (Exception ex)
            {
                rc.SetError(ReadContext, "Verarbeitungsfehler",
                    $"Beim Ausführen des Skriptes ist ein Fehler aufgetreten '{ex.Message}'. Die Ausführung wird abgebrochen."
                    + $"Variablenwerte: expandedVarName='{expandedVarName}' expandedNumber='{expandedNumber}' expandedFormat='{expandedFormat}'");
                return null;
            }
            return NextCommand;
        }
    }
}