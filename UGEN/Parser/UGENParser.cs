﻿// MIT License

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
