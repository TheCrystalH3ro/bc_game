using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;

namespace Assets.Scripts.Modules.Validation.Rules
{
    public class RequiredRule : IValidationRule
    {
        public ValidationResult Validate(object value)
        {
            var str = value?.ToString()?.Trim();

            if (string.IsNullOrEmpty(str)) {   
                return ValidationResult.Failure(ValidationRuleEnum.Required, "Value is required.");
            }

            return ValidationResult.Success();
        }
    }
}
