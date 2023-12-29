namespace FlowProtocol2.Commands
{
    using System.Text.RegularExpressions;
    using FlowProtocol2.Core;

    /// <summary>
    /// Implementiert den ForEachLine-Befehl
    /// </summary>
    public class CmdForEachLine : CmdLoopBaseCommand
    {
        public string VarName { get; set; }
        public string FileNameOrPath { get; set; }
        public string Take { get; set; }
        public string SectionVar { get; set; }
        private bool Initialized { get; set; }
        private List<LineItem> LineItems { get; set; }
        private string ExpandedVarName { get; set; }
        private string ExpandedSectionVar { get; set; }
        private int RSeed { get; set; }

        private class LineItem
        {
            public string LineContend { get; set; }
            public string Section { get; set; }
            public LineItem(string linecontend, string section)
            {
                LineContend = linecontend;
                Section = section;
            }
        }

        public static CommandParser GetComandParser()
        {
            return new CommandParser(@"^~ForEachLine ([A-Za-z0-9\$\(\)]*)\s*in\s*([^;]*)\s*(; Take=[A-Za-z0-9\$\(\)]*)?\s*(; SectionVar=[A-Za-z0-9\$\(\)]*)?",
                (rc, m) => CreateForEachLineCommand(rc, m));
        }

        private static CmdBaseCommand CreateForEachLineCommand(ReadContext rc, Match m)
        {
            CmdForEachLine cmd = new CmdForEachLine(rc);
            cmd.VarName = m.Groups[1].Value.Trim();
            cmd.FileNameOrPath = m.Groups[2].Value.Trim();
            cmd.Take = m.Groups[3].Value.Trim();
            cmd.SectionVar = m.Groups[4].Value.Trim();
            return cmd;
        }

        public CmdForEachLine(ReadContext readcontext) : base(readcontext)
        {
            VarName = string.Empty;
            FileNameOrPath = string.Empty;
            Take = string.Empty;
            SectionVar = string.Empty;
            Initialized = false;
            LineItems = new List<LineItem>();
            ExpandedVarName = string.Empty;
            ExpandedSectionVar = string.Empty;
            RSeed = 100;
        }

        public override CmdBaseCommand? Run(RunContext rc)
        {
            if (!Initialized)
            {
                string expandedFileNameOrPath = ReplaceVars(rc, FileNameOrPath);
                string absoluteFileName = ExpandPath(rc, expandedFileNameOrPath, out bool fileexists);
                if (!fileexists)
                {
                    rc.SetError(ReadContext, "Datei nicht gefunden",
                        $"Die Datei '{absoluteFileName}' konnte nicht gefunden werden. Die Skriptausführung wird abgebrochen.");
                    return null;
                }
                string expandedTake = ReplaceVars(rc, Take.Replace("; Take=", string.Empty)).Trim();
                int take = -1;
                if (!string.IsNullOrEmpty(expandedTake))
                {
                    bool takeOK = Int32.TryParse(expandedTake, out take);
                    if (!takeOK || take < 0)
                    {
                        rc.SetError(ReadContext, "Ungültiger Take-Parameter",
                            $"Der Ausdruck '{expandedTake}' kann nicht als Take-Parameter interpretiert werden. Die Skriptausführung wird abgebrochen.");
                        return null;
                    }
                }
                RSeed = GetRSeed(rc);
                ReadLineItems(absoluteFileName, take);
                ExpandedVarName = ReplaceVars(rc, VarName);
                ExpandedSectionVar = ReplaceVars(rc, SectionVar.Replace("; SectionVar=", string.Empty)).Trim();
                Initialized = true;
            }

            LinkAssociatedLoopCommand(rc, "ForEachLine");
            if (AssociatedLoopCommand != null)
            {
                if (LineItems.Any())
                {
                    var ret = LineItems.First();
                    LineItems.RemoveAt(0);
                    rc.InternalVars[ExpandedVarName] = ret.LineContend;
                    rc.InternalVars[ExpandedSectionVar] = ret.Section;
                    return NextCommand;
                }
                else
                {
                    Initialized = false;
                    return AssociatedLoopCommand.NextCommand;
                }
            }
            rc.SetError(ReadContext, "ForEachLine ohne Loop",
                "Dem DoWhile-Befehl kann kein Loop-Befehl auf gleicher Ebene zugeordnet werden. Die Bearbeitung wird abgebrochen.");
            return null;
        }

        private void ReadLineItems(string absoluteFileName, int take)
        {
            LineItems.Clear();
            using (StreamReader sr = new StreamReader(absoluteFileName))
            {
                string section = string.Empty;
                while (sr.Peek() != -1)
                {
                    string? line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string idxline = line.Trim();
                        if (idxline.StartsWith("//"))
                        {
                            // ignorieren
                        }
                        else if (idxline.StartsWith("[") && idxline.EndsWith("]"))
                        {
                            section = idxline.Substring(1, idxline.Length - 2);
                        }
                        else
                        {
                            LineItems.Add(new LineItem(idxline, section));
                        }
                    }
                }
            }
            Random tr = new Random(RSeed);
            while (take > 0 && LineItems.Count() > take)
            {
                int remidx = tr.Next(LineItems.Count);
                LineItems.RemoveAt(remidx);
            }
        }
    }
}