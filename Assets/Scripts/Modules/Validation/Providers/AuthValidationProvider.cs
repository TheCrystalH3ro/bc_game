using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Modules.Validation.Rules;

namespace Assets.Scripts.Modules.Validation.Providers
{
    public class AuthValidationProvider : ConfigurableValidationProvider
    {
        private readonly int _maxNameLength;
        private readonly int _maxEmailLength;
        private readonly int _maxPasswordLength;

        public AuthValidationProvider(int maxNameLength, int maxEmailLength, int maxPasswordLength)
        {
            _maxNameLength = maxNameLength;
            _maxEmailLength = maxEmailLength;
            _maxPasswordLength = maxPasswordLength;

            RegisterRules();
        }

        private void RegisterRules()
        {
            Register("login.username", _ => new RequiredRule());
            AddMessage("login.username", ValidationRuleEnum.Required, "Please provide an username to continue.");
        
            Register("login.password", _ => new RequiredRule());
            AddMessage("login.password", ValidationRuleEnum.Required, "Please enter your password to continue.");

            Register("register.username", _ => new RequiredRule());
            AddMessage("register.username", ValidationRuleEnum.Required, "Username is required. Please fill in this field.");
            Register("register.username", _ => new MaxLengthRule(_maxNameLength));
            AddMessage("register.username", ValidationRuleEnum.MaxLength, string.Format("Username is too long. Please use a shorter name (maximum characters: {0}).", _maxNameLength));

            Register("register.email", _ => new RequiredRule());
            AddMessage("register.email", ValidationRuleEnum.Required, "E-mail is required. Please fill in this field.");
            Register("register.email", _ => new MaxLengthRule(_maxEmailLength));
            AddMessage("register.email", ValidationRuleEnum.MaxLength, string.Format("E-mail is too long. Please use a shorter name (maximum characters: {0}).", _maxEmailLength));
            Register("register.email", _ => new EmailRule());
            AddMessage("register.email", ValidationRuleEnum.Email, "The email address you entered is not in the correct format. Please provide a valid email.");

            Register("register.password", _ => new RequiredRule());
            AddMessage("register.password", ValidationRuleEnum.Required, "Password is required. Please fill in this field.");
            Register("register.password", _ => new MaxLengthRule(_maxPasswordLength));
            AddMessage("register.password", ValidationRuleEnum.MaxLength, string.Format("Password is too long. Please use a shorter name (maximum characters: {0}).", _maxPasswordLength));

            Register("register.passwordConfirmation", context => new RequiredRule());
            AddMessage("register.passwordConfirmation", ValidationRuleEnum.Required, "Password confirmation is required. Please fill in this field.");
            Register("register.passwordConfirmation", context => new MaxLengthRule(_maxPasswordLength ));
            AddMessage("register.passwordConfirmation", ValidationRuleEnum.MaxLength, string.Format("Password confirmation is too long. Please use a shorter name (maximum characters: {0}).", _maxPasswordLength));
            Register("register.passwordConfirmation", context => new CompareRule(context as string));
            AddMessage("register.passwordConfirmation", ValidationRuleEnum.Compare, "Passwords do not match. Please make sure both passwords are identical.");
        }
    }
}
