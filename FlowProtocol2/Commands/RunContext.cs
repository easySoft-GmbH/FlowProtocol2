namespace FlowProtocol2.Commands
{
    using System;
    using System.Globalization;
    using FlowProtocol2.Core;
    public class RunContext
    {
        public Dictionary<string, string> BoundVars;
        public List<string> GivenKeys { get; set; }
        public Dictionary<string, string> InternalVars { get; set; }
        public List<ErrorElement> ErrorItems { get; set; }
        public IMForm InputForm { get; set; }
        public DocumentBuilder DocumentBuilder { get; set; }
        public List<IMBaseElement> InputItems => InputForm.InputItems;
        public string MyDomain {get; set;}
        public string MyBaseURL { get; set; }
        public string MyResultURL { get; set; }
        public string ScriptPath { get; set; }
        public string CurrentScriptPath { get; set; }
        public Dictionary<string, ScriptInfo> ScriptRepository { get; set; }
        public Stack<EntryPoint> ReturnStack { get; set; }
        public bool ExecuteNow { get; set; }
        public string BaseKey { get; set; }
        public int LoopStopCounter { get; set; }
        public int CommandStopCounter { get; set; }
        public CultureInfo Culture { get; set; }
        public List<string> LinkWhitelist { get; set; }
        public RunContext()
        {
            BoundVars = new Dictionary<string, string>();
            GivenKeys = new List<string>();
            InternalVars = new Dictionary<string, string>();
            ErrorItems = new List<ErrorElement>();
            DocumentBuilder = new DocumentBuilder();
            InputForm = new IMForm();
            MyDomain = string.Empty;
            MyBaseURL = string.Empty;
            MyResultURL = string.Empty;
            ScriptPath = string.Empty;
            CurrentScriptPath = string.Empty;
            ScriptRepository = new Dictionary<string, ScriptInfo>();
            ReturnStack = new Stack<EntryPoint>();
            ExecuteNow = false;
            BaseKey = string.Empty;
            Culture = CultureInfo.CurrentUICulture;
            LinkWhitelist = new List<string>();
            LoopStopCounter = 1000;
            CommandStopCounter = 20000;
        }
        public void SetError(ReadContext readcontext, string errorcode, string errortext)
        {
            ErrorItems.Add(new ErrorElement(readcontext, errorcode, errortext));
        }

        /// <summary>
        /// Prüft, ob ein Link in der Whitelist vorkommt.
        /// </summary>
        /// <param name="link">Der übergebene Link</param>
        /// <returns>True, wenn der Anfang des Links in der Whitelist aufgelistet ist oder die Liste leer ist.</returns>
        public bool IsOnWhitelist(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) return true;
            if (LinkWhitelist == null || !LinkWhitelist.Any()) return true;
            if (link.StartsWith(MyDomain)) return true;
            foreach (string wle in LinkWhitelist)
            {
                if (link.StartsWith(wle)) return true;
            }
            return false;
        }
    }

    public class EntryPoint
    {
        public CmdBaseCommand EntryCommand { get; set; }
        public string BaseKey { get; set; }
        public EntryPoint(CmdBaseCommand entrycommand, string basekey)
        {
            EntryCommand = entrycommand;
            BaseKey = basekey;
        }
    }
}