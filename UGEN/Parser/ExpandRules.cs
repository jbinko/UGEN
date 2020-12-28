using System;
using System.Collections.Generic;

namespace UGEN
{
    internal sealed partial class UGENBackend
    {
        private List<string> GetStringsFromTerminalRule(PatternRule parentRule, PatternTerminal pipeOperandRule)
        {
            // From sub-rule
            if (pipeOperandRule.Type == TerminalType.Rule)
                return ExpandRule(parentRule, pipeOperandRule.Rule);

            // From Referenced Rule
            if (pipeOperandRule.Type == TerminalType.Reference)
            {
                var referenceId = pipeOperandRule.Reference;
                if (!_cachedRules.ContainsKey(referenceId))
                    throw new InvalidOperationException(String.Format("Rule with id: '{0}' was not found in the cache.", referenceId));
                return _cachedRules[referenceId].Strings;
            }

            // From Static List
            return pipeOperandRule.Strings;
        }

        private List<string> ExpandRule(PatternRule parentRule, PatternCombinationRule rule)
        {
            var optionalFlagStack = new Stack<bool>();
            var stringsStack = new Stack<List<string>>();

            for (int i = 0; i < rule.CommaOperands.Count; i++)
            {
                var commaOperandRule = rule.CommaOperands[i];

                for (int j = 0; j < commaOperandRule.PipeOperands.Count; j++)
                {
                    var pipeOperandRule = commaOperandRule.PipeOperands[j];

                    stringsStack.Push(GetStringsFromTerminalRule(parentRule, pipeOperandRule));
                    optionalFlagStack.Push(pipeOperandRule.OptionalFlag);

                    if (j != 0)
                        stringsStack.Push(PipeOperator(stringsStack, optionalFlagStack));
                }

                if (i != 0)
                    stringsStack.Push(CommaOperator(stringsStack, optionalFlagStack));
            }

            return InjectEntityName(stringsStack, parentRule);
        }

        private List<string> PipeOperator(Stack<List<string>> stringsStack, Stack<bool> optionalFlagStack)
        {
            var strings = new List<string>();

            var r = stringsStack.Pop();
            var l = stringsStack.Pop();

            var lOptionalFlag = optionalFlagStack.Count > 0 ? optionalFlagStack.Pop() : false;
            var rOptionalFlag = optionalFlagStack.Count > 0 ? optionalFlagStack.Pop() : false;

            // This operator combines strings exclusively.
            // Into the set put strings from left only and after that strings from right part only

            // For this operation optional flags doesn't make to much sense and are not used
            // but must be poped from stack
            // A | B? is always equivalent to A | B because B? produces empty set

            foreach (var s1 in l)
                strings.Add(s1);
            foreach (var s2 in r)
                strings.Add(s2);

            return strings;
        }

        private List<string> CommaOperator(Stack<List<string>> stringsStack, Stack<bool> optionalFlagStack)
        {
            var strings = new List<string>();

            var r = stringsStack.Pop();
            var l = stringsStack.Pop();

            var lOptionalFlag = optionalFlagStack.Count > 0 ? optionalFlagStack.Pop() : false;
            var rOptionalFlag = optionalFlagStack.Count > 0 ? optionalFlagStack.Pop() : false;

            // This operator expands strings with each other. For each string on left side combine with each string on right side.
            // If optional flag operator is on left or right side expand strings in the way above plus strings without left or right side

            foreach (var s1 in l)
                foreach (var s2 in r)
                    strings.Add(s1 + " " + s2);

            if (rOptionalFlag)
            {
                foreach (var s2 in r)
                    strings.Add(s2);
            }

            if (lOptionalFlag)
            {
                foreach (var s1 in l)
                    strings.Add(s1);
            }

            // This is by design - we need this for expansion in higher expansions layers - at the very top empty strings are removed
            if (lOptionalFlag && rOptionalFlag)
                strings.Add("");

            return strings;
        }

        // If rule is entity type - decorate text with entity name for later use - this is internal format only
        // Format is: \u001E[MY_ENTITY_NAME]some my text\u001F
        // Format can also contain sub items in the same recursive way
        // E.g: \u001E[MY_ENTITY_NAME]some my text\u001E[MY_ENTITY_NAME2]some my text2\u001F\u001F
        private List<string> InjectEntityName(Stack<List<string>> stringsStack, PatternRule parentRule)
        {
            var strings = stringsStack.Pop();

            var entityName = parentRule.Type == PatternRuleType.Entity ? parentRule.ID : null;

            if (!String.IsNullOrWhiteSpace(entityName))
            {
                for (int i = 0; i < strings.Count; i++)
                    strings[i] = String.Format("{0}[{1}]{2}{3}", ENTITY_START, entityName, strings[i], ENTITY_END);
            }

            return strings;
        }

        // Extracts metadata about entities which are part of the string
        // Format is similar to xml markup. See above.
        // Extraction is via recursion
        private string ExtractEntities(string inString, int startIndex, List<Entity> entities)
        {
            // This is called repeatedly via recursion - until we can find some START marks
            var entityStartIdx = inString.IndexOf(ENTITY_START, startIndex);
            if (entityStartIdx == -1)
                return inString; // Go back and start processing in backwards or do nothing if no any markup

            // Entity name is between [] - just after ENTITY_START
            var braIdx = inString.IndexOf('[', entityStartIdx);
            if (braIdx == -1)
                throw new InvalidOperationException("Invalid index for decoding entity name(s) - expecting '['");

            var cketIdx = inString.IndexOf(']', braIdx);
            if (cketIdx == -1)
                throw new InvalidOperationException("Invalid index for decoding entity name(s) - expecting ']'");

            // Extract entity name and remove first part of markup (modify input string)
            // Recursion calls are not seeing them anymore
            var entityName = inString.Substring(braIdx + 1, cketIdx - braIdx - 1); // Between []
            var newString = inString.Remove(entityStartIdx, cketIdx - entityStartIdx + 1); // Remove ENTITY_START[some]

            // Now without entity metadata we need to use different start index where is our pure text
            var entityTextIdx = entityStartIdx;

            // Do recursion calls over stack until we can find ENTITY_START in modified string
            newString = ExtractEntities(newString, entityTextIdx, entities);

            // At this point there are NO any ENTITY_START anymore
            // We can slowly return backwards via stack calls - we just need to find pairing ENTITY_END
            // and remove remaining ENTITY_END
            var entityEndIdx = newString.IndexOf(ENTITY_END, entityTextIdx);
            if (entityEndIdx == -1)
                throw new InvalidOperationException("Invalid index for decoding entity name(s) - expecting '\u001F'");

            // Pure text described by entity and remove ENTITY_END from the original string
            var entityText = newString.Substring(entityTextIdx, entityEndIdx - entityTextIdx);
            newString = newString.Remove(entityEndIdx, 1);

            // Now without ENTITY_END we need to use different end text index where our pure text ends
            var entityTextEndIdx = entityEndIdx;

            // As we are going backwards we need to understand and store entity positions.
            // We need to know how many items/characters will be removed - are ahead of us in the string
            // to adjust positions
            int numOfRemainingEnds = CountRemainingEntityEnds(newString, entityTextIdx);

            entities.Add(new Entity
            {
                EntityText = entityText,
                EntityName = entityName,
                StartPos = entityTextIdx - numOfRemainingEnds,
                EndPos = entityTextEndIdx - numOfRemainingEnds
            });

            return newString;
        }

        // From Right to Left
        private int CountRemainingEntityEnds(string inString, int maxRightPos)
        {
            var numOfRemainingEnds = 0;
            var remainingEndIdx = maxRightPos;
            while ((remainingEndIdx = inString.LastIndexOf(ENTITY_END, Math.Max(remainingEndIdx - 1, 0))) != -1)
                numOfRemainingEnds++;
            return numOfRemainingEnds;
        }

        private const char ENTITY_START = '\u001E';
        private const char ENTITY_END = '\u001F';
    }
}
