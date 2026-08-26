using System.ComponentModel.DataAnnotations;

namespace icpms_client.UserControls.Customs.Forms.Validations.Size
{
    public class SizeValidationRule : ITextValidationRule
    {
        private readonly string _fieldName;
        private readonly int? _minSize;
        private readonly int? _maxSize;
        private ValidationResult? _IsValid;
        private SizeValidationType _type;

        public SizeValidationRule(int? minSize = null, int? maxSize = null, string fieldName = "This field", SizeValidationType type = SizeValidationType.String)
        {
            _minSize = minSize;
            _maxSize = maxSize;
            _fieldName = fieldName;
            _type = type;
        }
        public string? ErrorMessage { get; set; }
        public ValidationResult? IsValid { get => _IsValid; set => _IsValid = value; }

        public ValidationResult Validate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                var result = new ValidationResult($"{_fieldName} cannot be empty.");
                IsValid = result;
                return result;
            }

            if (_minSize != null || _maxSize != null)
            {
                if (_type == SizeValidationType.String)
                {
                    if (_maxSize != null && value.Length > _maxSize)
                    {
                        var result = new ValidationResult($"{_fieldName} must be less than {_maxSize} characters.");
                        IsValid = result;
                        return result;
                    }

                    if (_minSize != null && value.Length < _minSize)
                    {
                        var result = new ValidationResult($"{_fieldName} must be greater than {_minSize} characters.");
                        IsValid = result;
                        return result;
                    }

                    IsValid = ValidationResult.Success;
                    return ValidationResult.Success!;
                }
                else if (_type == SizeValidationType.Number)
                {
                    if (int.TryParse(value, out int numberValue))
                    {
                        if (_maxSize != null && numberValue > _maxSize)
                        {
                            var result = new ValidationResult($"{_fieldName} must be less than {_maxSize}.");
                            IsValid = result;
                            return result;
                        }

                        if (_minSize != null && numberValue < _minSize)
                        {
                            var result = new ValidationResult($"{_fieldName} must be greater than {_minSize}.");
                            IsValid = result;
                            return result;
                        }

                        IsValid = ValidationResult.Success;
                        return ValidationResult.Success!;
                    }
                    else
                    {
                        var result = new ValidationResult($"{_fieldName} must be a valid number.");
                        IsValid = result;
                        return result;
                    }
                }
            }

            IsValid = ValidationResult.Success;
            return ValidationResult.Success!;
        }
        
    }
    public enum SizeValidationType
    {
        String, Number
    }
}
