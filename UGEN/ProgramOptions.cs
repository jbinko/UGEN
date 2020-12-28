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
            "Copyright (C) 2020 Jiri Binko. All rights reserved.\n" +
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
                    new Option<string[]>(new[] { "--rules", "-r" }, "List of names of rules to display. Names of rules can be space, comma or semicolon separated"),
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
