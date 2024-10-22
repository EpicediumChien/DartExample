using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Method
{
    public class InputTypeString
    {
        public List<string> SubInputType(List<string> stringList)
        {
            if (stringList != null)
            {
                string T_strI = string.Empty;
                string T_strII = string.Empty;
                bool rc = false;
                for (int i = 0; i < stringList.Count; i++)
                {
                    T_strI = System.Text.RegularExpressions.Regex.Replace(stringList[i].ToString(), @"\d", string.Empty);

                    for (int j = 0; j < stringList.Count; j++)
                    {
                        if (i != j)
                        {
                            T_strII = System.Text.RegularExpressions.Regex.Replace(stringList[j].ToString(), @"\d", string.Empty);

                            if (T_strI.Equals(T_strII))
                            {
                                rc = true;
                                break;
                            }
                        }
                    }

                    if (!rc)
                        stringList[i] = T_strI;
                }
            }

            return stringList;
        }
    }
}
