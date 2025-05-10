using System.Text.RegularExpressions;
using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;

namespace Assets.Scripts.Modules.Validation.Rules
{
    public class EmailRule : IValidationRule
    {
        public ValidationResult Validate(object value)
        {
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            Regex regex = new(pattern);

            var str = value?.ToString();

            if (!regex.IsMatch(str)) {   
                return ValidationResult.Failure(ValidationRuleEnum.Email, "Email format is invalid.");
            }

            return ValidationResult.Success();
        }
    }
}
