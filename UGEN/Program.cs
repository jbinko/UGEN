// MIT License

// Copyright (c) 2020 Jiri Binko

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.CommandLine;
using System.Collections.Generic;

namespace UGEN
{
    internal static partial class Program
    {
        private static int ExecuteCommand(FileInfo file, FileInfo outFile, bool overwrite, string[] rulesToPrint, IConsole context, Cmd cmd)
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
                    Build(parser.Parse(stream), outFile, overwrite, rulesToPrint, context, cmd);
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

        private static void Build(dynamic dObject, FileInfo outFile, bool overwrite, string[] rulesToPrint, IConsole context, Cmd cmd)
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
                ProduceToConsole(rulesToPrint, cmd, backend);
            else
                ProduceToFile(outFile, overwrite, rulesToPrint, cmd, backend);
        }

        private static void ProduceToFile(FileInfo outFile, bool overwrite, string[] rulesToPrint, Cmd cmd, UGENBackend backend)
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
                PrintToFile(backend.Generated, rulesToPrint, outFile.FullName);

            Console.WriteLine(String.Format("The file '{0}' has been successfully created.", outFile.Name));
        }

        private static void ProduceToConsole(string[] rulesToPrint, Cmd cmd, UGENBackend backend)
        {
            if (cmd == Cmd.Create)
                LUISBatchTestingFile.Create(backend.Generated, null);
            else
                PrintToFile(backend.Generated, rulesToPrint, null);
        }

        private static void PrintToFile(List<CachedRule> generated, string[] rulesToPrint, string fileName)
        {
            var rulesToPrintSet = GetRulesToPrint(rulesToPrint);

            using (var sw = new StringWriter())
            {
                foreach (var r in generated)
                {
                    // Print only specific rules
                    if(rulesToPrintSet != null && rulesToPrintSet.Count > 0)
                    {
                        if (!rulesToPrintSet.Contains(r.Rule.ID))
                        {
                            sw.WriteLine(String.Format("Rule '{0}' - Print Skipped", r.Rule.ID));
                            continue;
                        }
                    }

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

        private static HashSet<string> GetRulesToPrint(string[] rulesToPrint)
        {
            if (rulesToPrint == null || rulesToPrint.Length <= 0)
                return null;

            var rules = new HashSet<string>();

            // can be space, comma or semicolon separated
            foreach (var r in rulesToPrint)
            {
                if (String.IsNullOrWhiteSpace(r))
                    continue;

                var separated = r.Split(' ', ',', ';');
                foreach (var s in separated)
                {
                    if (!String.IsNullOrWhiteSpace(s))
                        rules.Add(s.Trim());
                }
            }

            return rules;
        }
    }
}
