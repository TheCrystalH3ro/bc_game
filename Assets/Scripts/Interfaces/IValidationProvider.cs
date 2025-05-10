using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enums;

namespace Assets.Scripts.Interfaces
{
    public interface IValidationProvider
    {
        List<IValidationRule> GetRules(string fieldKey, object context = null);
        string GetMessage(string fieldKey, ValidationRuleEnum rule);
    }
}
