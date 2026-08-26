using System;
using System.ComponentModel.DataAnnotations;

namespace icpms_client.UserControls.Customs.Forms.Validations.Text
{
    public class NotEmptyTextValidationRule : ITextValidationRule
    {
        private readonly string _fieldName;
        private ValidationResult? _IsValid;

        // Constructor to set the field name (with a default value)
        public NotEmptyTextValidationRule(string? fieldName = null)
        {
            _fieldName = fieldName ?? "This field";
        }

        public string? ErrorMessage { get; set; }
        public ValidationResult? IsValid { get => _IsValid; set => _IsValid = value; }

        public ValidationResult Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Return validation error message with the provided field name
                var result = new ValidationResult($"{_fieldName} cannot be empty");
                IsValid = result;
                return result;
            }

            IsValid = ValidationResult.Success;
            // Return success validation result
            return ValidationResult.Success!;
        }
    }
}
