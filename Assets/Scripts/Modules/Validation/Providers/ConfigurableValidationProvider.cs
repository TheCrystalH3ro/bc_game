using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Modules.Validation.Providers
{
    public class ConfigurableValidationProvider : IValidationProvider
    {
        private readonly Dictionary<string, List<Func<object, IValidationRule>>> _ruleRegistry =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, Dictionary<ValidationRuleEnum, string>> _messageRegistry = 
            new(StringComparer.OrdinalIgnoreCase);

        public void Register(string fieldKey, Func<object, IValidationRule> ruleFactory)
        {
            if (!_ruleRegistry.ContainsKey(fieldKey))
                _ruleRegistry[fieldKey] = new List<Func<object, IValidationRule>>();

            _ruleRegistry[fieldKey].Add(ruleFactory);
        }

        public void AddMessage(string fieldKey, ValidationRuleEnum ruleType, string message)
        {
            if (!_messageRegistry.ContainsKey(fieldKey))
                _messageRegistry[fieldKey] = new Dictionary<ValidationRuleEnum, string>();

            _messageRegistry[fieldKey].Add(ruleType, message);
        }

        public void ClearRules(string fieldKey)
        {
            _ruleRegistry.Remove(fieldKey);
            _messageRegistry.Remove(fieldKey);
        }

        public List<IValidationRule> GetRules(string fieldKey, object context = null)
        {
            if (!_ruleRegistry.ContainsKey(fieldKey))
                return new();

            return _ruleRegistry[fieldKey]
                .Select(factory => factory(context))
                .ToList();
        }

        public string GetMessage(string fieldKey, ValidationRuleEnum rule)
        {
            if (!_messageRegistry.ContainsKey(fieldKey))
                return "";

            if (!_messageRegistry[fieldKey].ContainsKey(rule))
                return "";

            return _messageRegistry[fieldKey][rule];
        }
    }
}
