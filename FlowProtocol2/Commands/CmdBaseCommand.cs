using System.Text.RegularExpressions;
using FlowProtocol2.Core;

namespace FlowProtocol2.Commands
{
    public abstract class CmdBaseCommand
    {
        public CmdBaseCommand? NextCommand { get; set; }
        public CmdBaseCommand? PreviousCommand { get; set; }
        public ReadContext ReadContext { get; set; }
        public int Indent => ReadContext.Indent;
        protected CmdBaseCommand(ReadContext readcontext)
        {
            ReadContext = readcontext;
        }
        public abstract CmdBaseCommand? Run(RunContext rc);
        public void SetNextCommand(CmdBaseCommand nextcommand)
        {
            if (nextcommand != this)
            {
                NextCommand = nextcommand;
                nextcommand.PreviousCommand = this;
            }
        }
        protected string ReplaceVars(RunContext rc, string input)
        {
            if (input.Contains('$'))
            {
                input = input.Replace("$BaseKey", rc.BaseKey);
                string compareInput = input;
                do // Wende die Variablen-Ersetzung wiederholt an, bis sich nix mehr verändert
                {
                    compareInput = input;                    
                    foreach (var v in rc.InternalVars.OrderByDescending(x => x.Key))
                    {
                        if (!string.IsNullOrWhiteSpace(v.Key))
                        {
                            input = input.Replace("$" + v.Key, v.Value);
                        }
                    }
                } while (compareInput != input && input.Contains('$'));
                // Systemvariablen                
                input = input.Replace("$NewGuid", Guid.NewGuid().ToString());
                input = input.Replace("$CRLF", "\r\n");
                input = input.Replace("$LF", "\n");
                input = input.Replace("$ScriptFilePath", ReadContext.ScriptFilePath);                
                input = input.Replace("$BaseURL", rc.MyBaseURL);
                input = input.Replace("$ResultURL", rc.MyResultURL);
                input = input.Replace("$ScriptPath", rc.ScriptPath);
                input = input.Replace("$CurrentScriptPath", rc.CurrentScriptPath);
                if (input.Contains("$LineNumber-"))
                {
                    Regex lnm = new Regex(@"\$LineNumber-([0-9]*)");
                    while (lnm.IsMatch(input))
                    {
                        Match m = lnm.Match(input);
                        bool subOK = Int32.TryParse(m.Groups[1].Value, out int sub);
                        if (subOK)
                        {
                            input = input.Replace(m.Groups[0].Value, (ReadContext.LineNumber - sub).ToString());
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                input = input.Replace("$LineNumber", ReadContext.LineNumber.ToString());
                if (input.Contains("$Chr"))
                {
                    for (int i = 1; i < 255; i++)
                    {
                        input = input.Replace($"$Chr({i})", Convert.ToChar(i).ToString());
                    }
                }
            }
            return input;
        }

        public T? GetNextCommand<T>(Func<T, bool> predicate, Func<CmdBaseCommand, bool> stopcrit)
            where T : CmdBaseCommand => GetCommand<T>(predicate, stopcrit, c => c.NextCommand);
        public T? GetPreviousCommand<T>(Func<T, bool> predicate, Func<CmdBaseCommand, bool> stopcrit)
            where T : CmdBaseCommand => GetCommand<T>(predicate, stopcrit, c => c.PreviousCommand);

        public T? GetFirstCommand<T>(Func<T, bool> predicate, Func<CmdBaseCommand, bool> stopcrit)
            where T : CmdBaseCommand
        {
            CmdBaseCommand top = GetPreviousCommand<CmdBaseCommand>(c => c.PreviousCommand == null, c => false) ?? this;
            return top.GetNextCommand<T>(predicate, stopcrit);
        }

        private T? GetCommand<T>(Func<T, bool> predicate, Func<CmdBaseCommand, bool> stopcrit,
            Func<CmdBaseCommand, CmdBaseCommand?> searchdirection)
            where T : CmdBaseCommand
        {
            CmdBaseCommand? cmdidx = searchdirection(this);
            while (cmdidx != null && !stopcrit(cmdidx))
            {
                T? cmdT = cmdidx as T;
                if (cmdT != null && predicate(cmdT))
                {
                    return cmdT;
                }
                cmdidx = searchdirection(cmdidx);
            }
            return null;
        }

        public CmdBaseCommand? GetNextSameOrHigherLevelCommand()
            => GetCommand<CmdBaseCommand>(c => c.Indent <= this.Indent, c => false, c => c.NextCommand);

        public List<T> GetNexCommands<T>(Func<T, bool> predicate, Func<CmdBaseCommand, bool> stopcrit)
            where T : CmdBaseCommand
        {
            List<T> ret = new List<T>();
            CmdBaseCommand? cmdidx = NextCommand;
            while (cmdidx != null && !stopcrit(cmdidx))
            {
                T? cmdT = cmdidx as T;
                if (cmdT != null && predicate(cmdT))
                {
                    ret.Add(cmdT);
                }
                cmdidx = cmdidx.NextCommand;
            }
            return ret;
        }

        protected bool EvaluateExpression(RunContext rc, string expression, out ErrorElement? err)
        {
            err = null;
            foreach (var disterm in expression.Split("||"))
            {
                bool bdis = EvaluateDisTerm(rc, disterm, out err);
                if (err != null) return false;
                if (bdis) return true;
            }
            return false;
        }

        private bool EvaluateDisTerm(RunContext rc, string expression, out ErrorElement? err)
        {
            err = null;
            foreach (var conterm in expression.Split("&&"))
            {
                bool bcon = EvaluateConTerm(rc, conterm, out err);
                if (err != null) return false;
                if (!bcon) return false;
            }
            return true;
        }

        private bool EvaluateConTerm(RunContext rc, string expression, out ErrorElement? err)
        {
            err = null;
            expression = expression.Trim();
            string expandedexpression = ReplaceVars(rc, expression).Trim();
            if (expandedexpression == "1" || expandedexpression == "true") return true;
            if (expandedexpression == "0" || expandedexpression == "false") return false;
            if (expression.StartsWith("!?$")) return !rc.InternalVars.ContainsKey(ReplaceVars(rc, expression[3..]));
            if (expression.StartsWith("?$")) return rc.InternalVars.ContainsKey(ReplaceVars(rc, expression[2..]));
            bool result = false;
            if (CheckCompSTerm(rc, expression, "==", (x, y) => x == y, out result, out err)) return result;
            if (CheckCompSTerm(rc, expression, "!=", (x, y) => x != y, out result, out err)) return result;
            if (CheckCompDTerm(rc, expression, "<>", (x, y) => x != y, out result, out err)) return result;
            if (CheckCompDTerm(rc, expression, "<=", (x, y) => x <= y, out result, out err)) return result;
            if (CheckCompDTerm(rc, expression, ">=", (x, y) => x >= y, out result, out err)) return result;
            if (CheckCompDTerm(rc, expression, "<", (x, y) => x < y, out result, out err)) return result;
            if (CheckCompDTerm(rc, expression, ">", (x, y) => x > y, out result, out err)) return result;
            if (CheckCompSTerm(rc, expression, "!~", (x, y) => !x.Contains(y), out result, out err)) return result;
            if (CheckCompSTerm(rc, expression, "~", (x, y) => x.Contains(y), out result, out err)) return result;            
            err = new ErrorElement(ReadContext, "Ungültiger Vergleichsterm",
                $"Der Ausdruck '{expression}' kann nicht als Vergleichsterm interpretiert werden.");
            return false;
        }

        private bool CheckCompSTerm(RunContext rc, string expression, string scop, Func<string, string, bool> lcop,
            out bool result, out ErrorElement? err)
        {
            err = null;
            Regex regCompTerm = new Regex(@"(.*)" + scop + "(.*)");
            if (regCompTerm.IsMatch(expression))
            {
                var m = regCompTerm.Match(expression);
                string value1 = ReplaceVars(rc, m.Groups[1].Value.Trim());
                string value2 = ReplaceVars(rc, m.Groups[2].Value.Trim());
                result = lcop(value1, value2);
                return true;
            }
            result = false;
            return false;
        }
        private bool CheckCompDTerm(RunContext rc, string expression, string scop, Func<double, double, bool> lcop,
            out bool result, out ErrorElement? err)
        {
            err = null;
            Regex regCompTerm = new Regex(@"(.*)" + scop + "(.*)");
            if (regCompTerm.IsMatch(expression))
            {
                var m = regCompTerm.Match(expression);
                string value1 = ReplaceVars(rc, m.Groups[1].Value.Trim());
                string value2 = ReplaceVars(rc, m.Groups[2].Value.Trim());

                bool w1OK = double.TryParse(value1, out double dblvalue1);
                bool w2OK = double.TryParse(value2, out double dblvalue2);
                if (!w1OK)
                {
                    err = new ErrorElement(ReadContext, "Ungültiger numerischer Ausdruck",
                        $"Der Ausdruck '{value1}' kann nicht als Gleitkommazahl interpretiert werden.");
                }
                else if (!w2OK)
                {
                    err = new ErrorElement(ReadContext, "Ungültiger numerischer Ausdruck",
                        $"Der Ausdruck '{value2}' kann nicht als Gleitkommazahl interpretiert werden.");
                }
                else
                {
                    result = lcop(dblvalue1, dblvalue2);
                    return true;
                }
            }
            result = false;
            return false;
        }

        /// <summary>
        /// Expandiert einen Dateinamen oder Teilpfad zu einem absoluten Pfad.
        /// </summary>
        /// <param name="rc">RunContext</param>
        /// <param name="pathorname">Ein Dateiname oder ein mit ".\" beginnender Teilpfad</param>
        /// <param name="fileexists">Gibt zurück, ob die Datein existiert.</param>
        /// <returns>Der expandierte Pfad</returns>
        protected string ExpandPath(RunContext rc, string pathorname, out bool fileexists)
        {
            string ret = string.Empty;
            if (pathorname.StartsWith("." + Path.DirectorySeparatorChar))
            {
                ret = $"{rc.ScriptPath}{pathorname[1..]}";
            }
            else
            {
                ret = $"{rc.CurrentScriptPath}{Path.DirectorySeparatorChar}{pathorname}";
            }
            fileexists = false;
            System.IO.FileInfo fi = new System.IO.FileInfo(ret);
            if (fi != null && fi.Exists) fileexists = true;
            return ret;
        }

        protected int GetRSeed(RunContext rc)
        {
            int rseed = 100;
            if (rc.BoundVars.ContainsKey("_rseed"))
            {
                bool ok = Int32.TryParse(rc.BoundVars["_rseed"], out rseed);
            }
            else
            {
                rseed = new Random().Next();
                rc.BoundVars["_rseed"] = rseed.ToString();
            }
            rc.GivenKeys.Add("_rseed");
            return rseed;
        }
    }
}