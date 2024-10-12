using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    public class ComboBoxTextValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value is string custName)
            {
                if (String.IsNullOrWhiteSpace(custName))
                {
                    return new ValidationResult(false, "Cannot be empty");
                }
            }


            return new ValidationResult(true, null);
        }
    }
}
