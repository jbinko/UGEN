using System;
using System.IO;
using System.Collections.Generic;

using Antlr4.Runtime;

namespace UGEN
{
    internal sealed class UGENParser
    {
        public dynamic Parse(string inpuText)
        {
            if (String.IsNullOrEmpty(inpuText))
                inpuText = String.Empty;
            return Parse(new AntlrInputStream(inpuText));
        }

        public dynamic Parse(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream), $"Stream input argument cannot be null value");
            return Parse(new AntlrInputStream(inputStream));
        }

        public dynamic Parse(TextReader textReader)
        {
            if (textReader == null)
                throw new ArgumentNullException(nameof(textReader), $"Text reader input argument cannot be null value");
            return Parse(new AntlrInputStream(textReader));
        }

        private dynamic Parse(AntlrInputStream stream)
        {
            var errorListener = new UGENErrorListener(this);

            var lexer = new UGENGenerated.UGENLexer(stream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);

            var tokens = new CommonTokenStream(lexer);

            var parser = new UGENGenerated.UGENParser(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            parser.ErrorHandler = new BailErrorStrategy(this);

            var dtNow = DateTime.Now;
            var tree = parser.start();
            if (HasErrors)
                return null;
            //Console.WriteLine((DateTime.Now - dtNow).TotalMilliseconds);            
            //Console.WriteLine(tree.ToStringTree(parser));

            dtNow = DateTime.Now;
            var retObject = (dynamic)new UGENVisitor(this).Visit(tree);
            if (HasErrors)
                return null;
            //Console.WriteLine((DateTime.Now - dtNow).TotalMilliseconds);
            
            return retObject;
        }

        public void OnSyntaxError(Action<string> action)
        {
            _onSyntaxError = action;
        }
        internal void FireOnSyntaxError(string message)
        {
            _errors.Add(message);
            _onSyntaxError?.Invoke(message);
        }

        private Action<string> _onSyntaxError;
        public bool HasErrors { get { return _errors.Count > 0; } }
        public string[] Errors { get { return _errors.ToArray(); } }
        private List<string> _errors = new List<string>();
    }
}
