using System;

using Antlr4.Runtime;

namespace UGEN
{
    internal sealed class BailErrorStrategy : DefaultErrorStrategy
    {
        public BailErrorStrategy(UGENParser parser)
        {
            _parser = parser;
        }

        public override IToken RecoverInline(Parser recognizer)
        {
            var message = $"Syntax error line: {recognizer.CurrentToken.Line}, column: {recognizer.CurrentToken.Column} near token '{recognizer.CurrentToken.Text}'.";
            _parser.FireOnSyntaxError(message);
            throw new InputMismatchException(recognizer);
        }

        public override void Sync(Parser recognizer)
        {
        }

        private UGENParser _parser;
    }
}
