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
using System.Linq;
using System.Collections.Generic;

namespace UGEN
{
    internal sealed class CachedRule
    {
        public PatternRule Rule { get; set; }
        // This contains strings including injected metadata about Entity - are also used on the fly for calculation
        // It is for internal use only
        public List<string> Strings { get; set; }
        // This is final public product
        public List<StringEntity> StringEntities { get; set; }
    }

    internal sealed class StringEntity
    {
        public string Text { get; set; }
        public List<Entity> Entities { get; set; }
    }

    internal sealed class Entity
    {
        public string EntityText { get; set; }
        public string EntityName { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
    }

    internal sealed partial class UGENBackend
    {
        public UGENBackend(List<PatternRule> rules)
        {
            if (rules == null)
                throw new NullReferenceException("Backend is missing input data.");

            _rules = rules;
        }

        private bool ValidateIDs()
        {
            bool isValid = true;

            // Check for Rule IDs, duplicities
            var ids = new Dictionary<string, PatternRule>();
            foreach (var rule in _rules)
            {
                var id = rule.ID;
                if (!ids.ContainsKey(id))
                    ids.Add(rule.ID, rule);
                else
                {
                    FireOnModelValidationError(String.Format("Rule with identifier '{0}' already exists. Check for duplicity, consider new identifier.", id));
                    isValid = false;
                }
            }

            var notUsedRulesByIds = new Dictionary<string, PatternRule>(ids);

            // Check for missing ID References
            WalkerIDReferences((parentId, id) => {

                if (!ids.ContainsKey(id))
                {
                    FireOnModelValidationError(String.Format("Missing rule with identifier '{0}'. Check rule definitions.", id));
                    isValid = false;
                }

                // Remove IDs, which were referenced at least once
                if (notUsedRulesByIds.ContainsKey(id))
                    notUsedRulesByIds.Remove(id);
            });

            // Warnings for not used IDs
            foreach (var notUsedRule in notUsedRulesByIds)
            {
                // Skip Intent rules - they are root rules
                if (notUsedRule.Value.Type != PatternRuleType.Intent)
                {
                    FireOnModelValidationWarning(String.Format(
                        "Rule with identifier '{0}' is NOT in use. Consider removal of the rule or mark it as INTENT rule.", notUsedRule.Value.ID));
                }
            }

            return isValid;
        }

        private bool ResolveDependencies()
        {
            var dependencies = new Dictionary<string, List<string>>();
            WalkerIDReferences((parentId, id) => {
                RegisterDependency(dependencies, parentId, id);
            });

            var sorted = dependencies.TopologySort();

            if(sorted.Cycles.Count > 0)
            {
                var cycleIds = new List<string>();
                foreach(var item in sorted.Cycles)
                {
                    foreach (var id in item)
                        cycleIds.Add(id);
                }

                var ids = String.Join(',', cycleIds.Distinct().ToArray());
                FireOnModelValidationError(String.Format("Circular dependency detected for following rule(s): '{0}'.", ids));
            }

            _dependenciesOrder = sorted.Sorted;
            return sorted.Cycles.Count == 0;
        }

        private void RegisterDependency(Dictionary<string, List<string>> dependencies, string parentId, string id)
        {
            if (!dependencies.ContainsKey(parentId))
                dependencies.Add(parentId, new List<string> { id });
            else
            {
                if (!dependencies[parentId].Contains(id))
                    dependencies[parentId].Add(id);
            }
        }

        private void ExpandRules()
        {
            var rulesForExpansion = new List<CachedRule>();

            // List for Expansion in the dependencies order (Leafs first)
            foreach (var ruleId in _dependenciesOrder)
            {
                foreach (var rule in _rules)
                {
                    if (rule.ID == ruleId)
                    {
                        rulesForExpansion.Add(RegisterCachedRule(rule));
                        break;
                    }
                }
            }

            // Now we just need to add all the other missing rules - rules which had no dependencies detected
            foreach (var rule in _rules)
            {
                if (!_cachedRules.ContainsKey(rule.ID))
                    rulesForExpansion.Add(RegisterCachedRule(rule));
            }

            foreach (var rule in rulesForExpansion)
                ExpandCachedRule(rule);
        }

        private CachedRule RegisterCachedRule(PatternRule rule)
        {
            var cachedRule = new CachedRule { Rule = rule };
            _cachedRules.Add(rule.ID, cachedRule);
            return cachedRule;
        }

        private void ExpandCachedRule(CachedRule cachedRule)
        {
            Console.WriteLine("Producing rule: {0}", cachedRule.Rule.ID);
            CleanProducedStrings(cachedRule, ExpandRule(cachedRule.Rule, cachedRule.Rule.Body));
        }

        // Removes produced empty strings (which are correctly produced because we need them for subsequent ops) and trim any white spaces
        // Also remove Injected Entity Metadata and extract entity metadata
        private void CleanProducedStrings(CachedRule cachedRule, List<string> inStrings)
        {
            var outStrings = new List<string>();
            var outStringEntities = new List<StringEntity>();

            foreach (var s in inStrings)
            {
                var newString = s.Trim();

                if (String.IsNullOrWhiteSpace(newString))
                    continue;

                // In outStrings Keep string with injected metadata
                outStrings.Add(newString);

                // In outStringEntities Keep PURE strings with NO injected metadata
                // Extract Metadata into entities
                var entities = new List<Entity>();
                newString = ExtractEntities(newString, 0, entities);

                outStringEntities.Add(new StringEntity
                {
                    Text = newString,
                    Entities = entities
                });
            }

            cachedRule.Strings = outStrings;
            cachedRule.StringEntities = outStringEntities;
        }

        private void WalkerIDReferences(Action<string, string> id)
        {
            foreach (var rule in _rules)
                WalkerIDReferencesRecursions(rule.ID, rule.Body, id);
        }

        private void WalkerIDReferencesRecursions(string parentId, PatternCombinationRule rule, Action<string, string> id)
        {
            foreach (var commaOperandRule in rule.CommaOperands)
            {
                foreach (var pipeOperandRule in commaOperandRule.PipeOperands)
                {
                    if (pipeOperandRule.Type == TerminalType.Rule)
                        WalkerIDReferencesRecursions(parentId, pipeOperandRule.Rule, id);
                    else if (pipeOperandRule.Type == TerminalType.Reference)
                        id.Invoke(parentId, pipeOperandRule.Reference);
                }
            }
        }

        private bool Validate()
        {
            var isValid = ValidateIDs();
            if (!isValid)
                return isValid;

            isValid = ResolveDependencies();
            if (!isValid)
                return isValid;

            return true;
        }

        public bool Generate()
        {
            var isValid = Validate();
            if (!isValid)
                return isValid;

            ExpandRules();

            return true;
        }

        public List<CachedRule> Generated { get { return new List<CachedRule>(_cachedRules.Values); } }

        public void OnModelValidationError(Action<string> error)
        {
            _onModelError = error;
        }
        internal void FireOnModelValidationWarning(string message)
        {
            _onModelWarning?.Invoke("Warning: " + message);
        }

        public void OnModelValidationWarning(Action<string> warning)
        {
            _onModelWarning = warning;
        }

        internal void FireOnModelValidationError(string message)
        {
            _onModelError?.Invoke("Error: " + message);
        }

        private Action<string> _onModelError = null;
        private Action<string> _onModelWarning = null;

        private List<PatternRule> _rules = null;
        private List<string> _dependenciesOrder = null;
        private Dictionary<string, CachedRule> _cachedRules = new Dictionary<string, CachedRule>();
    }
}
