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

using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

namespace UGEN
{
    internal sealed class UGENErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public UGENErrorListener(UGENParser parser)
        {
            _parser = parser;
        }

        /*
         * This method is called by the parser when a full-context prediction has a unique result.
         * Each full-context prediction which does not result in a syntax error will call either ANTLRErrorListener.reportContextSensitivity
         * or ANTLRErrorListener.reportAmbiguity.
         * For prediction implementations that only evaluate full-context predictions when an SLL conflict is found (including the default ParserATNSimulator implementation),
         * this method reports cases where SLL conflicts were resolved to unique full-context predictions, i.e. the decision was context-sensitive. This report does not 
         * necessarily indicate a problem, and it may appear even in completely unambiguous grammars.
         * configs may have more than one represented alternative if the full-context prediction algorithm does not evaluate predicates before beginning the full-context prediction. 
         * In all cases, the final prediction is passed as the prediction argument.
         * Note that the definition of "context sensitivity" in this method differs from the concept in DecisionInfo.contextSensitivities. 
         * This method reports all instances where an SLL conflict occurred but LL parsing produced a unique result, whether or not that unique result matches the minimum 
         * alternative in the SLL conflicting set. This method is not used by lexers.
         *  recognizer - the parser instance
         *  dfa - the DFA for the current decision
         *  startIndex - the input index where the decision started
         *  stopIndex - the input index where the context sensitivity was finally determined
         *  prediction - the unambiguous result of the full-context prediction
         *  configs - the ATN configuration set where the unambiguous prediction was determined
         */
        public override void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
        {
            return;
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine($"Ambiguity start index: {startIndex} resolved at index: {stopIndex}, {recognizer.Context.ToStringTree(recognizer)}");
            //Console.ResetColor();
        }

        /*
         * This method is called when an SLL conflict occurs and the parser is about to use the full context information to make an LL decision.
         * If one or more configurations in configs contains a semantic predicate, the predicates are evaluated before this method is called.
         * The subset of alternatives which are still viable after predicates are evaluated is reported in conflictingAlts.
         * This method is not used by lexers.
         *  recognizer - the parser instance
         *  dfa - the DFA for the current decision
         *  startIndex - the input index where the decision started
         *  stopIndex - the input index where the SLL conflict occurred
         *  conflictingAlts - The specific conflicting alternatives. If this is null, the conflicting alternatives are all alternatives represented in configs.
         *      At the moment, conflictingAlts is non-null (for the reference implementation, but Sam's optimized version can see this as null).
         *  conflictState - the ATN configuration set where the SLL conflict was detected
         */
        public override void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
        {
            return;
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine($"Attempting full context start index: {startIndex} conflict at index: {stopIndex}, {recognizer.Context.ToStringTree(recognizer)}");
            //Console.ResetColor();
        }

        /*
         * This method is called by the parser when a full-context prediction has a unique result.
         * Each full-context prediction which does not result in a syntax error will call either ANTLRErrorListener.reportContextSensitivity or
         * ANTLRErrorListener.reportAmbiguity.
         * For prediction implementations that only evaluate full-context predictions when an SLL conflict is found (including the default ParserATNSimulator implementation), 
         * this method reports cases where SLL conflicts were resolved to unique full-context predictions, i.e. the decision was context-sensitive. 
         * This report does not necessarily indicate a problem, and it may appear even in completely unambiguous grammars.
         * configs may have more than one represented alternative if the full-context prediction algorithm does not evaluate predicates before beginning the 
         * full-context prediction. In all cases, the final prediction is passed as the prediction argument.
         * Note that the definition of "context sensitivity" in this method differs from the concept in DecisionInfo.contextSensitivities. 
         * This method reports all instances where an SLL conflict occurred but LL parsing produced a unique result, whether or not that unique 
         * result matches the minimum alternative in the SLL conflicting set.
         * This method is not used by lexers.
         *  recognizer - the parser instance
         *  dfa - the DFA for the current decision
         *  startIndex - the input index where the decision started
         *  stopIndex - the input index where the context sensitivity was finally determined
         *  prediction - the unambiguous result of the full-context prediction
         *  configs - the ATN configuration set where the unambiguous prediction was determined
         */
        public override void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
        {
            return;
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine($"Context Sensitivity start index: {startIndex} determined at index: {stopIndex}, {recognizer.Context.ToStringTree(recognizer)}");
            //Console.ResetColor();
        }

        /*
         * Upon syntax error, notify any interested parties. This is not how to recover from errors or compute error messages. 
         * ANTLRErrorStrategy specifies how to recover from syntax errors and how to compute error messages. This listener's job is simply to emit a computed message, 
         * though it has enough information to create its own message in many cases.
         * The RecognitionException is non-null for all syntax errors except when we discover mismatched token errors that we can recover from in-line, 
         * without returning from the surrounding rule (via the single token insertion and deletion mechanism).
         *  recognizer - What parser got the error. From this object, you can access the context as well as the input stream.
         *  offendingSymbol - The offending token in the input token stream, unless recognizer is a lexer (then it's null). 
         *      If no viable alternative error, e has token at which we started production for the decision.
         *  line - The line number in the input where the error occurred.
         *  charPositionInLine - The character position within that line where the error occurred.
         *  msg - The message to emit.
         *  e - The exception generated by the parser that led to the reporting of an error. It is null in the case where the parser was able to
         *      recover in line without exiting the surrounding rule.
         */
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var message = $"Syntax error line: {line}, column: {charPositionInLine}, {msg}.";
            _parser.FireOnSyntaxError(message);
        }

        private UGENParser _parser;
    }
}
