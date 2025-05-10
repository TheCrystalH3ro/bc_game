using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;

namespace Assets.Scripts.Modules.Validation.Rules
{
    public class MaxLengthRule : IValidationRule
    {
        private readonly int _max;

        public MaxLengthRule(int max)
        {
            _max = max;
        }

        public ValidationResult Validate(object value)
        {
            var str = value?.ToString() ?? string.Empty;

            if (str.Length > _max) {
                return ValidationResult.Failure(ValidationRuleEnum.MaxLength, $"Maximum length is {_max} characters.");
            }

            return ValidationResult.Success();
        }
    }
}
