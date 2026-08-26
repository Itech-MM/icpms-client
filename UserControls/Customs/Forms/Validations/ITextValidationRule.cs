using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace icpms_client.UserControls.Customs.Forms.Validations
{
    public interface ITextValidationRule
    {
        ValidationResult Validate(string value);
        string? ErrorMessage { get; set; }
        ValidationResult? IsValid {  get; set; }
    }
}
