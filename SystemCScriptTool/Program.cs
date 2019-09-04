using System;
using System.IO;
using System.Linq;
using SystemCScriptManager;

namespace SystemCScriptTool
{
    static class Program
    {
        static void Main(string[] args)
        {
            foreach (var File in args)
            {
                string FileName = Path.GetFileNameWithoutExtension(File);
                if (FileName.ToLower().EndsWith("_dump"))
                {
                    string ScrPath = Path.Combine(Path.GetDirectoryName(File), FileName.Substring(0, FileName.Length - "_dump".Length) + ".txt");
                    Insert(ScrPath, File);
                }
                else
                {
                    string DumpPath = Path.Combine(Path.GetDirectoryName(File), FileName + "_dump.txt");
                    Dump(File, DumpPath);
                }
            }
        }

        public static void Dump(string Script, string Output)
        {
            Console.WriteLine("Dumping: {0}", Path.GetFileName(Script));
            ParserWrapper Parser = new ParserWrapper(Script);
            File.WriteAllLines(Output, Parser.Import());
        }

        public static void Insert(string Script, string Dump)
        {
            Console.WriteLine("Importing: {0}", Path.GetFileName(Script));
            var Lines = File.ReadAllLines(Dump);

            ParserWrapper Parser = new ParserWrapper(Script);
            var Original = Parser.Import();
            if (Math.Abs(Lines.Length - Original.Length) > 1)
                throw new Exception("Dump Length Missmatch");

            var tmp = new string[Original.Length];
            for (int i = 0; i < tmp.Length; i++)
                tmp[i] = i >= Lines.Length ? Original[i] : Lines[i];

            Parser.Export(tmp);
        }

    }
}
