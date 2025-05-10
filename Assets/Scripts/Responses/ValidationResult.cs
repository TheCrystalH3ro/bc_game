using System.Collections;
using Assets.Scripts.Enums;

namespace Assets.Scripts.Responses
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public ValidationRuleEnum Rule { get;}
        public string ErrorMessage { get; set; }

        private ValidationResult(bool isValid, ValidationRuleEnum rule = ValidationRuleEnum.None, string errorMessage = null)
        {
            IsValid = isValid;
            Rule = rule;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new(true);

        public static ValidationResult Failure(ValidationRuleEnum rule, string message) => new(false, rule, message);
    }
}
