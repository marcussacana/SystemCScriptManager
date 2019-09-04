using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SystemCScriptManager
{
    public class ScriptParser
    {
        public Encoding Encoding = Encoding.GetEncoding(932);
        string[] Script;
        string ScriptPath;
        string WorkDir;
        LinkEntry[] Entries;
        public ScriptParser(string ScriptPath)
        {
            this.ScriptPath = ScriptPath;
            WorkDir = Path.GetDirectoryName(ScriptPath);
            Script = File.ReadAllLines(ScriptPath, Encoding);
            if (!WorkDir.EndsWith("\\") && !WorkDir.EndsWith("/"))
                WorkDir += Path.DirectorySeparatorChar;
        }

        public string[] Import()
        {
            Entries = ParseLinks();
            List<string> Strings = new List<string>();
            foreach (var Link in Entries)
            {
                if (!Link.IsLineLink())
                    continue;
                if (Link.LinkInfo.Count == 0)
                    continue;
                string Line = string.Join("\r\n", Link.Lines);
                Strings.Add(Line);
            }

            return Strings.ToArray();
        }

        public void Export(string[] Strings)
        {

            Dictionary<string, List<LinkEntry>> NewEntries = new Dictionary<string, List<LinkEntry>>();
            List<string> NewScript = new List<string>();
            NewScript.AddRange(Script);
            NewScript.Add("//******************* SystemCScriptManager Insertion *******************");
            NewScript.Add(string.Empty);

            int CurStr = 0;
            for (int i = 0; i < Entries.Length; i++)
            {
                var Link = Entries[i];
                if (!Link.IsLineLink() || Link.LinkInfo.Count == 0)
                {
                    if (!NewEntries.ContainsKey(Link.SPTName))
                        NewEntries[Link.SPTName] = new List<LinkEntry>();
                    NewEntries[Link.SPTName].Add(Link);
                    continue;
                }

                string OriLine = string.Join("\r\n", Link.Lines);
                string NewLine = Strings[CurStr++];
                if (NewLine == OriLine)
                {
                    if (!NewEntries.ContainsKey(Link.SPTName))
                        NewEntries[Link.SPTName] = new List<LinkEntry>();
                    NewEntries[Link.SPTName].Add(Link);
                    continue;
                }
                
                string[] NewLines = NewLine.Replace("\r\n", "\n").Split('\n');
#if DEBUG
                NewScript.Add($"//Line Link: From {Link.LinkInfo.Line} to {NewScript.Count} ({Link.Lines.Length} Lines to {NewLines.Length} Lines)");
#endif
                Link.LinkInfo.Line = (uint)NewScript.Count;
                Link.LinkInfo.Count = (uint)NewLines.Length;

                foreach (var Line in NewLines)
                    NewScript.Add(Line);
                NewScript.Add(string.Empty);

                if (!NewEntries.ContainsKey(Link.SPTName))
                    NewEntries[Link.SPTName] = new List<LinkEntry>();
                NewEntries[Link.SPTName].Add(Link);
            }

            foreach (var Pair in NewEntries)
            {
                string OutPath = WorkDir + Pair.Key;
                if (File.Exists(OutPath) && !File.Exists(OutPath + ".bak"))
                    File.Move(OutPath, OutPath + ".bak");

                using (StructWriter Writer = new StructWriter(OutPath))
                {
                    var SPT = new SPTFormat();
                    SPT.Entries = (from x in Pair.Value select x.LinkInfo).ToArray();
                    Writer.WriteStruct(ref SPT);
                }
            }


            if (File.Exists(ScriptPath) && !File.Exists(ScriptPath + ".bak"))
                File.Move(ScriptPath, ScriptPath + ".bak");

            File.WriteAllLines(ScriptPath, NewScript.ToArray(), Encoding);
        }

        LinkEntry[] ParseLinks()
        {
            List<string> ParsedSpts = new List<string>();
            List<LinkEntry> Entries = new List<LinkEntry>();
            foreach (var Block in GetBlocks())
            {
                var LblParts = Block.Label.Split('_');

                var SptList = new List<string>();
                if (Block.Label.Substring(0, 2) == "SC") //SC_A0100_00_A0110_00	
                {
                    var FileA = $"{LblParts[1]}_{LblParts[2]}.spt";
                    var FileB = $"{LblParts[3]}_{LblParts[4]}.spt";

                    if (!File.Exists(WorkDir + FileA))
                        throw new FileNotFoundException(FileA);
                    //if (!File.Exists(WorkDir + FileB))
                    //    throw new FileNotFoundException(FileB);

                    SptList.Add(FileA);
                    //SptList.Add(FileB);
                }
                else if (Block.Label.Substring(0, 2) == "SS") //SS_A0010_30_02_40_50
                {
                    string Base = LblParts[1];

                    var FileName = $"{Base}_{LblParts[2]}.spt";
                    if (!File.Exists(WorkDir + FileName))
                        throw new FileNotFoundException(FileName);
                    SptList.Add(FileName);

                    foreach (var Part in LblParts.Skip(4))
                    {
                        FileName = $"{Base}_{Part}.spt";
                        if (!File.Exists(WorkDir + FileName))
                            throw new FileNotFoundException(FileName);
                        SptList.Add(FileName);
                    }
                }
                else
                {
                    throw new NotImplementedException(Block.Label.Substring(0, 2) + " Label not supported.");
                }

                SptList = SptList.Distinct().ToList();
                SptList = new List<string>((from x in SptList where !ParsedSpts.Contains(x) select x).ToArray());
                ParsedSpts.AddRange(SptList);

                foreach (string File in SptList)
                {
                    var SPTData = new SPTFormat();

                    using (StructReader Reader = new StructReader(WorkDir + File))
                        Reader.ReadStruct(ref SPTData);

                    List<LinkEntry> Links = new List<LinkEntry>();
                    foreach (var Info in SPTData.Entries)
                    {
                        LinkEntry Entry = new LinkEntry();
                        Entry.Lines = new string[0];
                        if (Info.IsLineLink())
                        {
                           // if (!Block.ContainsLine(Info.Line))
                           //     continue;
                            string[] Lines = new string[Info.Count];
                            for (int i = 0; i < Lines.Length; i++)
                                Lines[i] = Script[Info.Line + i];
                            Entry.Lines = Lines;
                        }
                        Entry.LinkInfo = Info;
                        Entry.SPTName = File;
                        Links.Add(Entry);
                    }

                    Entries.AddRange(Links);
                }

            }
            int SPTs = (from x in Entries select x.SPTName).Distinct().Count();
            return Entries.ToArray();
        }
        ScriptBlock[] GetBlocks() {
            List<ScriptBlock> Blocks = new List<ScriptBlock>();
            List<string> Lines = new List<string>();
            bool CreateNew = true;
            ScriptBlock Current = new ScriptBlock();
            for (uint i = 0; i < Script.Length; i++)
            {
                if (CreateNew)
                {
                    CreateNew = false;
                    Current = new ScriptBlock();
                    Current.FirstLinePos = i;
                }

                string Line = Script[i];
                if (Line.StartsWith("//"))
                {
                    Lines.Add(Line);
                    continue;
                }

                if (Line.StartsWith("***"))
                {
                    string Label = Line.Substring(3);
                    Label = Label.Substring(0, Label.IndexOf("//")).Trim();
                    if (Current.Label == null)
                    {
                        Current.Label = Label;
                        Lines.Add(Line);
                    }
                    else
                    {
                        Current.Lines = Lines.ToArray();
                        Blocks.Add(Current);
                        CreateNew = true;
                        i--;
                    }
                    continue;
                }

                Lines.Add(Line);
            }

            return Blocks.ToArray();
        }
    }

    static class Extensions
    {
        public static bool ContainsLine(this LinkEntry Entry, uint Line) => Entry.LinkInfo.ContainsLine(Line);

        public static bool ContainsLine(this SPTEntry Entry, uint Line)
        {
            return Entry.Line >= Line && Line <= Entry.Line + Entry.Count;
        }

        public static bool ContainsLine(this ScriptBlock Entry, uint Line)
        {
            return Entry.FirstLinePos >= Line && Line <= Entry.FirstLinePos + Entry.Lines.Length;
        }

        public static bool IsLineLink(this LinkEntry Entry) => Entry.LinkInfo.IsLineLink();
        public static bool IsLineLink(this SPTEntry Entry)
        {
            if (Entry.Line == uint.MaxValue || Entry.Count == uint.MaxValue || Entry.Count == 0)
                return false;
            if (Entry.Action > 10)
                return false;
            return true;
        }
    }

    internal struct ScriptBlock
    {
        public string Label;
        public uint FirstLinePos;
        public string[] Lines;
    }

    internal struct LinkEntry {
        public SPTEntry LinkInfo;
        public string SPTName;
        public string[] Lines;
    }

    internal struct SPTFormat
    {
        [PArray, StructField]
        public SPTEntry[] Entries;
    }

    internal struct SPTEntry
    {
        public uint Action;
        private uint UnkA;
        private uint UnkB;
        private uint UnkC;
        public uint Line;
        public uint Count;
        private uint UnkD;
        private uint UnkE;
    }
}
