using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using icpms_client.UserControls.Customs.Forms.Validations.Form;

namespace icpms_client.UserControls.Customs.Forms.Validations.Core
{
    public class FormValidator
    {
        private readonly Dictionary<string, FormField> _fields = [];
        public void AddField(string fieldName, FormField validationRule)
        {
            _fields[fieldName] = validationRule;
        }
        public bool Validate()
        {
            bool isValid = true;

            foreach (var field in _fields)
            {
                FormField validationRule = field.Value;
                var result = validationRule.Validate();
                if (result != null && result != ValidationResult.Success) {
                    isValid = false;
                }

            }

            return isValid;
        }

        public void Clear()
        {
            foreach (var field in _fields)
            {
                field.Value.IsError = false;
            }
        }
    }
}
