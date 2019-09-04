using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemCScriptManager
{
    public class ParserWrapper : ScriptParser
    {
        string[] Lines;
        int[] Counts;
		public ParserWrapper(string ScriptPath) : base(ScriptPath)
        {

        }

		public new string[] Import()
        {
            Lines = base.Import();
            Counts = new int[Lines.Length];

            List<string> WrapperLines = new List<string>();
            for (int i = 0; i < Lines.Length; i++)
            {
                var Splited = Lines[i].Replace("\r\n", "\n").Split('\n');
                Counts[i] = Splited.Length;
                foreach (var SLine in Splited)
                    WrapperLines.Add(SLine);
            }

            return WrapperLines.ToArray();
        }


		public new void Export(string[] Strings)
        {
            List<string> NewList = new List<string>();
            for (int i = 0, x = 0, Count = 0; i < Lines.Length; i++, x += Count)
            {
                Count = Counts[i];
                var NewLines = Strings.Skip(x).Take(Count);
                NewLines = NewLines.SelectMany(y => y.Replace("\\n", "\x0").Split('\x0'));
                NewList.Add(string.Join("\r\n", NewLines).Replace("\r\n<--", ""));//.Replace("\r\n<--", " ") will put a space between the lines
            }

            base.Export(NewList.ToArray());
        }
    }
}
