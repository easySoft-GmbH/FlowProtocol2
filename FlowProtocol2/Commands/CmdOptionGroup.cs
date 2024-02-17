namespace FlowProtocol2.Commands
{
    using System.Text.RegularExpressions;
    using FlowProtocol2.Core;

    /// <summary>
    /// Implementiert den Option-Befehl
    /// </summary>
    public class CmdOptionGroup : CmdInputBaseCommand
    {
        public string Key { get; set; }
        public string Promt { get; set; }
        public CmdOptionValue? SelectedOptionCommand { get; set; }
        private int InputIndex { get; set; }

        public static CommandParser GetComandParser()
        {
            return new CommandParser(@"^\?([A-Za-z0-9\$\(\)]*[']?):(.*)", (rc, m) => CreateOptionGroupCommand(rc, m));
        }

        private static CmdBaseCommand CreateOptionGroupCommand(ReadContext rc, Match m)
        {
            CmdOptionGroup cmd = new CmdOptionGroup(rc);
            cmd.Key = m.Groups[1].Value.Trim();
            cmd.Promt = m.Groups[2].Value.Trim();
            return cmd;
        }

        public CmdOptionGroup(ReadContext readcontext) : base(readcontext)
        {
            Key = string.Empty;
            Promt = string.Empty;
            SelectedOptionCommand = null;
            InputIndex = 0;
        }

        public override CmdBaseCommand? Run(RunContext rc)
        {
            CmdOptionValue? firstOptionValue = GetNextCommand<CmdOptionValue>(c => true, c => c.Indent < this.Indent);
            if (firstOptionValue == null)
            {
                rc.SetError(ReadContext, "Option ohne Werte",
                    "Für die Optionsgruppe wurde kein Wert angegeben. Die Ausführung wird abgebrochen.");
                return null;
            }
            IMOptionGroupElement ogroup = new IMOptionGroupElement();
            string expandedKey = ReplaceVars(rc, Key).Trim();
            string plainKey = expandedKey;
            if (string.IsNullOrEmpty(expandedKey))
            {
                expandedKey = "'";
            }
            if (!string.IsNullOrEmpty(rc.BaseKey))
            {
                expandedKey = rc.BaseKey + "_" + expandedKey;
            }
            if (expandedKey.EndsWith("'"))
            {
                if (InputIndex == 0)
                {
                    InputIndex = GetPreviousCommands<CmdInputBaseCommand>(c => true, c => false).Count + 1;
                }
                expandedKey = expandedKey.Replace("'", "_" + InputIndex.ToString());
            }
            ogroup.Key = expandedKey;
            ogroup.Promt = ReplaceVars(rc, Promt).Trim();

            string selectedKey = string.Empty;
            if (rc.BoundVars.ContainsKey(expandedKey))
            {
                selectedKey = rc.BoundVars[expandedKey];
            }

            CmdOptionValue? xOption = null;
            SelectedOptionCommand = null;
            var allOptions = GetNextCommands<CmdOptionValue>(
                    c => c.Indent == firstOptionValue.Indent,
                    c => c.Indent < firstOptionValue.Indent);
            int optioncount = 0;
            foreach (var idxo in allOptions)
            {
                IMOptionValue ov = new IMOptionValue(ogroup);
                string optionkey = idxo.Key;
                optioncount++;
                if (string.IsNullOrEmpty(optionkey))
                {
                    optionkey = "_" + optioncount.ToString();
                }
                ov.Key = optionkey;
                ov.Promt = ReplaceVars(rc, idxo.Promt);
                ogroup.Options.Add(ov);
                if (!string.IsNullOrEmpty(selectedKey) && ov.Key == selectedKey)
                {
                    SelectedOptionCommand = idxo;
                }
                if (ov.Key == "x")
                {
                    xOption = idxo;
                }
                idxo.ParentOptionGroupCommand = this;
            }
            if (!string.IsNullOrEmpty(selectedKey) && SelectedOptionCommand == null)
            {
                SelectedOptionCommand = xOption;
            }
            if (SelectedOptionCommand != null)
            {
                rc.GivenKeys.Add(expandedKey);
            }
            else
            {
                rc.BoundVars[expandedKey] = string.Empty;
                rc.InputForm.AddInputItem(ogroup);
                AssociatedInputElement = ogroup;
            }
            if (rc.BoundVars.ContainsKey(expandedKey))
            {
                rc.InternalVars[plainKey] = rc.BoundVars[expandedKey];
            }
            return NextCommand;
        }
    }
}