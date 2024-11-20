using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DDPM.UI.Common.Method
{
    public class TextString
    {
        public bool CheckChar(string ch)
        {
            //if (char.IsLetterOrDigit(ch))
            //    return true;
            //if (ch == ' ' || ch == '@' || ch == 'e')
            //    return true;
            //return false;
            Regex regex = new Regex("^[0-9a-zA-Z @-]+$");
            return regex.IsMatch(ch);
        }
    }
}
