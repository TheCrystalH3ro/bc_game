using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;

namespace Assets.Scripts.Modules.Validation.Rules
{
    public class CompareRule : IValidationRule
    {
        private readonly object _compared_to;

        public CompareRule(object compared_to)
        {
            _compared_to = compared_to;
        }

        public ValidationResult Validate(object value)
        {

            if (value.Equals(_compared_to)) {   
                return ValidationResult.Failure(ValidationRuleEnum.Compare, "Values does not match.");
            }

            return ValidationResult.Success();
        }
    }
}
