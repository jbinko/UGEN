using System;
using System.IO;
using System.CommandLine;
using System.Collections.Generic;

namespace UGEN
{
    internal static partial class Program
    {
        private static int ExecuteCommand(FileInfo file, FileInfo outFile, bool overwrite, IConsole context, Cmd cmd)
        {
            try
            {
                var parser = new UGENParser();
                parser.OnSyntaxError(e =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ResetColor();
                });

                using (var stream = File.OpenRead(file.FullName))
                    Build(parser.Parse(stream), outFile, overwrite, context, cmd);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();

                System.Diagnostics.Trace.WriteLine(e.ToString());
                return -1;
            }

            return 0;
        }

        private static void Build(dynamic dObject, FileInfo outFile, bool overwrite, IConsole context, Cmd cmd)
        {
            if (dObject == null)
                return; // All errors dumped already - no action needed

            var backend = new UGENBackend((List<PatternRule>)dObject);

            backend.OnModelValidationError(e =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
            });

            backend.OnModelValidationWarning(w =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(w);
                Console.ResetColor();
            });

            var isValid = backend.Generate();
            if (!isValid)
                return;

            if (outFile == null)
                ProduceToConsole(cmd, backend);
            else
                ProduceToFile(outFile, overwrite, cmd, backend);
        }

        private static void ProduceToFile(FileInfo outFile, bool overwrite, Cmd cmd, UGENBackend backend)
        {
            if (outFile.Exists && !overwrite)
            {
                backend.FireOnModelValidationError(String.Format(
                    "The file '{0}' already exists. Use flag '--force' if you wish to overwrite this existing file.", outFile.FullName));
                return;
            }

            if (cmd == Cmd.Create)
                LUISBatchTestingFile.Create(backend.Generated, outFile.FullName);
            else
                PrintToFile(backend.Generated, outFile.FullName);

            Console.WriteLine(String.Format("The file '{0}' has been successfully created.", outFile.Name));
        }

        private static void ProduceToConsole(Cmd cmd, UGENBackend backend)
        {
            if (cmd == Cmd.Create)
                LUISBatchTestingFile.Create(backend.Generated, null);
            else
                PrintToFile(backend.Generated, null);
        }

        private static void PrintToFile(List<CachedRule> generated, string fileName = null)
        {
            using (var sw = new StringWriter())
            {
                foreach (var r in generated)
                {
                    if (r.Rule.Type == PatternRuleType.Default)
                        sw.WriteLine(String.Format("Rule '{0}':", r.Rule.ID));
                    else
                        sw.WriteLine(String.Format("Rule '{0}' ({1}):", r.Rule.ID, r.Rule.Type));

                    foreach (var s in r.StringEntities)
                        sw.WriteLine("  " + s.Text);
                }

                if (String.IsNullOrWhiteSpace(fileName))
                    System.Console.Write(sw.ToString());
                else
                    System.IO.File.WriteAllText(fileName, sw.ToString());
            }
        }
    }
}
