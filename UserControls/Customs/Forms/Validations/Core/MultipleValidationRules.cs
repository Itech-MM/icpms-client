using System.ComponentModel.DataAnnotations;

namespace icpms_client.UserControls.Customs.Forms.Validations.Core
{
    public class MultipleValidationRules : ITextValidationRule
    {
        private readonly List<ITextValidationRule> _validationRules = new();
        private ValidationResult? _isValid;

        public void AddRule(ITextValidationRule rule)
        {
            _validationRules.Add(rule);
        }

        public string? ErrorMessage { get; set; }

        public ValidationResult? IsValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                ErrorMessage = value?.ErrorMessage; // Update ErrorMessage based on validation result
            }
        }

        public ValidationResult Validate(string value)
        {
            foreach (var rule in _validationRules)
            {
                var result = rule.Validate(value);
                if (result != ValidationResult.Success)
                {
                    IsValid = result; // Set IsValid to the failing rule's result
                    return result;
                }
            }

            IsValid = ValidationResult.Success; // All rules passed
            return ValidationResult.Success!;
        }
    }

}
