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
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace UGEN
{
    internal static partial class Program
    {
        private const string DESCRIPTION =
            "Generates utterances from input based on combinatorial language.\n" +
            "Copyright (c) 2020 Jiri Binko. All rights reserved.\n" +
            "This tool is released under MIT License.\n" +
            "https://github.com/jbinko/UGEN\n\n" +
            "Use 'ugen --version' to display the current version.";

        private enum Cmd
        {
            Create,
            Print
        }

        private static async Task<int> Main(string[] args)
        {
            var createCmd = new Command("create", "Creates utterances based on input rules file")
                {
                    new Argument<FileInfo>("file", "Input file with UGEN rules and declarations").ExistingOnly(),
                    new Option<FileInfo>(new[] { "--out", "-o" }, "Output file name to write out produced content. If not specified, content will be output to standard output"),
                    new Option<bool>(new[] { "--force", "-f" }, "If --out flag is provided with the path to an existing file, overwrites that file"),
                };
            createCmd.Handler = CommandHandler.Create<FileInfo, FileInfo, bool, IConsole>(CreateCmdHandler);

            var printCmd = new Command("print", "Prints utterances based on input rules file")
                {
                    new Argument<FileInfo>("file", "Input file with UGEN rules and declarations").ExistingOnly(),
                    new Option<FileInfo>(new[] { "--out", "-o" }, "Output file name to write out produced content. If not specified, content will be output to standard output"),
                    new Option<bool>(new[] { "--force", "-f" }, "If --out flag is provided with the path to an existing file, overwrites that file"),
                    new Option<string[]>(new[] { "--rules", "-r" }, "List of names of rules to display. Names of rules can be space or comma separated"),
                };
            printCmd.Handler = CommandHandler.Create<FileInfo, FileInfo, bool, string[], IConsole>(PrintCmdHandler);

            var rootCmd = new RootCommand(DESCRIPTION) { createCmd, printCmd };

            return await rootCmd.InvokeAsync(args);
        }

        private static int CreateCmdHandler(FileInfo file, FileInfo @out, bool force, IConsole context)
        {
            return ExecuteCommand(file, @out, force, null, context, Cmd.Create);
        }

        private static int PrintCmdHandler(FileInfo file, FileInfo @out, bool force, string[] rules, IConsole context)
        {
            return ExecuteCommand(file, @out, force, rules, context, Cmd.Print);
        }
    }
}
