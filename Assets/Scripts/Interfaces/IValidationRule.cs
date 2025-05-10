using Assets.Scripts.Responses;

namespace Assets.Scripts.Interfaces
{
    public interface IValidationRule
    {
        ValidationResult Validate(object value);
    }
}
