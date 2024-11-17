using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.EABroker
{
    public class CustomNameValidationRule : ValidationRule
    {
        public int MaxChars { get; set; }

        //Rule:
        //1 Maximun 30 characters
        //2 Valid characters: [a~z],[A~Z],[0~9],
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            //Check max length
            if (((string)value).Length > MaxChars)
            {
                return new ValidationResult(false, $"Use a maximum of {MaxChars} characters");
            }
            //
            return new ValidationResult(true, null);
        }

    }
}
