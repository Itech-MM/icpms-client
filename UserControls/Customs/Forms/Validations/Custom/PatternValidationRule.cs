using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace icpms_client.UserControls.Customs.Forms.Validations.Custom
{
    public class PatternValidationRule : ITextValidationRule
    {
        string _pattern;
        string _fieldName;
        private ValidationResult? _IsValid;
        public PatternValidationRule(string pattern, string? fieldName, string? errorMessage = null)
        {
            _pattern = pattern;
            _fieldName = fieldName ?? "This field";
            ErrorMessage = errorMessage ?? null;
        }

        public string? ErrorMessage { get; set; }
        public ValidationResult? IsValid { get => _IsValid; set => _IsValid = value; }

        public ValidationResult Validate(string value)
        {
            if (!Regex.IsMatch(value, _pattern))
            {
                var message = ErrorMessage ?? $"{_fieldName} format is invalid.";
                IsValid = new ValidationResult(message);
                return new ValidationResult(message);
            }

            IsValid = ValidationResult.Success;
            return ValidationResult.Success!;
        }
    }
}
