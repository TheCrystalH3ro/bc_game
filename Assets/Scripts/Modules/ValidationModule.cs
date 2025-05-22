using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;

namespace Assets.Scripts.Modules
{
    public class ValidationModule
    {
        private static ValidationModule  _instance;
    
        public static ValidationModule Singleton
        { 
            get
            {
                _instance ??=  new();

                return _instance;
            }
        }

        public ValidationResult Validate(string value, IValidationRule rule)
        {    
            ValidationResult result = rule.Validate(value);

            return result;
        }

        public ValidationResult Validate(string value, List<IValidationRule> rules)
        { 
            ValidationResult result = ValidationResult.Success();

            foreach(IValidationRule rule in rules) {
                result = rule.Validate(value);

                if(!result.IsValid) {
                    return result;
                }
            }

            return result;
        }
    }
}