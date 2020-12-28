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
using System.Collections.Generic;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using UGENGenerated;

namespace UGEN
{
    internal enum TerminalType
    {
        Reference,
        Strings,
        Rule
    }

    internal sealed class PatternTerminal
    {
        public TerminalType Type { get; set; }
        public string Reference { get; set; }
        public List<string> Strings { get; set; }
        public PatternCombinationRule Rule { get; set; }
        public bool OptionalFlag { get; set; }
    }

    internal enum PatternRuleType
    {
        Default,
        Entity,
        Intent
    }

    internal sealed class PatternRule
    {
        public PatternRuleType Type { get; set; } = PatternRuleType.Default;
        public string ID { get; set; }
        public PatternCombinationRule Body { get; set; }
    }

    internal sealed class PatternCombinationRule
    {
        public List<PatternExclusiveRule> CommaOperands { get; } = new List<PatternExclusiveRule>();
    }

    internal sealed class PatternExclusiveRule
    {
        public List<PatternTerminal> PipeOperands { get; } = new List<PatternTerminal>();
    }

    internal sealed class UGENVisitor : UGENBaseVisitor<object>
    {
        public UGENVisitor(UGENParser parser)
        {
            _parser = parser;
        }

        public override object VisitPatternExprTerm([NotNull] UGENGenerated.UGENParser.PatternExprTermContext context)
        {
            if (context.STRING_TUPLE() == null && context.IDENTIFIER() == null && context.patternExprList() == null)
                throw new NullReferenceException("Invalid context parameters - expression terminal.");

            if (context.STRING_TUPLE() != null)
                return new PatternTerminal { Type = TerminalType.Strings, Strings = ExtractStringTuple(context.STRING_TUPLE()) };

            if (context.IDENTIFIER() != null)
                return new PatternTerminal { Type = TerminalType.Reference, Reference = ExtractReference(context.IDENTIFIER()) };

            var rule = (PatternCombinationRule)Visit(context.patternExprList());
            if (rule == null)
                throw new NullReferenceException("Invalid child pattern combination rule - expression terminal.");
            return new PatternTerminal { Type = TerminalType.Rule, Rule = rule };
        }

        public override object VisitPatternExprOperatorOptional([NotNull] UGENGenerated.UGENParser.PatternExprOperatorOptionalContext context)
        {
            if (context.patternExprTerm() == null || context.optionalFlag() == null)
                throw new NullReferenceException("Invalid context parameters - operator optional.");

            var patternTerminal = (PatternTerminal)Visit(context.patternExprTerm());
            if (patternTerminal == null)
                throw new NullReferenceException("Invalid pattern terminal - operator optional.");

            var optionalFlagValue = ExtractString(context.optionalFlag());
            patternTerminal.OptionalFlag = !String.IsNullOrWhiteSpace(optionalFlagValue);
            return patternTerminal;
        }

        public override object VisitPatternExprOperatorPipe([NotNull] UGENGenerated.UGENParser.PatternExprOperatorPipeContext context)
        {
            if (context.patternExprOperatorOptional() == null || context.patternExprOperatorOptional().Length <= 0)
                throw new NullReferenceException("Invalid context parameters - operator pipe.");

            var patternExclusiveRule = new PatternExclusiveRule();

            foreach (var item in context.patternExprOperatorOptional())
            {
                var patternTerminal = (PatternTerminal)Visit(item);
                if (patternTerminal != null)
                    patternExclusiveRule.PipeOperands.Add(patternTerminal);
            }

            if (patternExclusiveRule.PipeOperands.Count <= 0)
                throw new InvalidOperationException("Unexpected number of terminal rules.");

            return patternExclusiveRule;
        }

        public override object VisitPatternExprList([NotNull] UGENGenerated.UGENParser.PatternExprListContext context)
        {
            if (context.patternExprOperatorPipe() == null || context.patternExprOperatorPipe().Length <= 0)
                throw new NullReferenceException("Invalid context parameters - expressions list.");

            var patternCombinationRule = new PatternCombinationRule();

            foreach (var item in context.patternExprOperatorPipe())
            {
                var patternExclusiveRule = (PatternExclusiveRule)Visit(item);
                if (patternExclusiveRule != null)
                    patternCombinationRule.CommaOperands.Add(patternExclusiveRule);
            }

            if (patternCombinationRule.CommaOperands.Count <= 0)
                throw new InvalidOperationException("Unexpected number of pattern exclusive rules.");

            return patternCombinationRule;
        }

        public override object VisitPattern([NotNull] UGENGenerated.UGENParser.PatternContext context)
        {
            if (/*context.TYPE() == null || */context.IDENTIFIER() == null || context.patternExprList() == null)
                throw new NullReferenceException("Invalid context parameters - pattern rule.");

            var patternRule = new PatternRule
            {
                Type = ExtractPatternRuleType(context.TYPE()),
                ID = ExtractString(context.IDENTIFIER()),
                Body = (PatternCombinationRule)Visit(context.patternExprList())
            };

            if (patternRule.Body.CommaOperands.Count <= 0)
                throw new InvalidOperationException("Unexpected number of pattern combination rules.");

            Patterns.Add(patternRule);
            return Patterns;
        }

        public override object VisitStart([NotNull] UGENGenerated.UGENParser.StartContext context)
        {
            base.VisitStart(context);
            return Patterns;
        }

        private string ExtractString(IParseTree parseTree)
        {
            if (parseTree == null)
                throw new NullReferenceException("Parse tree is not expected to be null - Extract String.");

            var text = parseTree.GetText();
            if (text == null)
                throw new NullReferenceException("Text is not expected to be null - Extract String.");

            return text.Trim();
        }

        private PatternRuleType ExtractPatternRuleType(IParseTree parseTree)
        {
            if (parseTree == null)
                return PatternRuleType.Default;

            var type = ExtractString(parseTree).ToLower();

            if (type == "entity")
                return PatternRuleType.Entity;

            if (type == "intent")
                return PatternRuleType.Intent;

            throw new InvalidOperationException(String.Format("Unknown rule type: '{0}' - Extract Pattern Rule Type.", type));
        }

        private string ExtractReference(IParseTree parseTree)
        {
            var reference = ExtractString(parseTree);

            if (String.IsNullOrWhiteSpace(reference))
                throw new InvalidOperationException("Reference/ID is not expected to be null or empty - Extract Reference.");

            return reference;
        }

        private List<string> ExtractStringTuple(IParseTree parseTree)
        {
            var stringTuple = ExtractString(parseTree);

            if (String.IsNullOrWhiteSpace(stringTuple))
                throw new InvalidOperationException("String tuple is not expected to be null or empty - Extract String Tuple.");

            var len = stringTuple.Length; // Min len for this token must be > 2 because of two brackets we are going to remove
            if (len <= 2)
                throw new InvalidOperationException("String tuple with brackets is not expected to be null or empty - Extract String Tuple.");

            // Remove Brackets at beginning and the end
            var withoutBrackets = stringTuple.Substring(1, len - 2);

            // TODO: Escape commas
            var strings = withoutBrackets.Split(',');

            // Purify strings
            var stringList = new List<string>();
            foreach (var s in strings)
            {
                var pureString = s.Trim();
                if (String.IsNullOrWhiteSpace(pureString))
                    continue;
                stringList.Add(pureString);
            }

            return stringList;
        }

        private List<PatternRule> Patterns { get; } = new List<PatternRule>();
        private UGENParser _parser;
    }
}
