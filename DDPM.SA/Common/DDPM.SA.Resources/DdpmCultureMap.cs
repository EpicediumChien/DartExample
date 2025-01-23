using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Resources
{
    public class DdpmCultureMap
    {
        //The default mapped CultureInfo which is Mapped from CultureInfo.CurrentUICulture
        private static readonly CultureInfo _currentCultureInfo;


        //The static constructor will not be called until the first reference to the class is made.
        //So add a code in DdpmHomePlugin to get MappedCultureInfo and write to the log file.
        static DdpmCultureMap()
        {
            _currentCultureInfo = MapCultureInfo(CultureInfo.CurrentUICulture);
        }
        public static CultureInfo MappedCultureInfo => _currentCultureInfo;

        /// <summary>
        /// Input CultureIno and convert to the mapped cultureInfo which is supported by DDPM.
        /// </summary>
        /// <param name="cultureIn"></param>
        public static CultureInfo MapCultureInfo(CultureInfo cultureIn)
        {
            //"ar": All convert to "ar-SA"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("ar-SA");
            }
            //"de": All convert to "de-DE"
            if (cultureIn.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("de-DE");
            }
            //"es"
            if (cultureIn.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase))
            {
                switch (cultureIn.Name)
                {
                    //es-MX
                    case "es-MX":
                        return CultureInfo.CreateSpecificCulture("es-MX");
                    //es-419 (Latin America)
                    case "es-419":
                    case "es-AR":
                    case "es-BO":
                    case "es-BZ":
                    case "es-CL":
                    case "es-CO":
                    case "es-CR":
                    case "es-CU":
                    case "es-DO":
                    case "es-EC":
                    case "es-GT":
                    case "es-HN":
                    case "es-NI":
                    case "es-PA":
                    case "es-PE":
                    case "es-PR":
                    case "es-PY":
                    case "es-SV":
                    case "es-UY":
                    case "es-VE":
                        return CultureInfo.CreateSpecificCulture("es-419");
                    default:
                        return CultureInfo.CreateSpecificCulture("es-ES");
                } //switch
            }// if "es"
            //"fr"
            if (cultureIn.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("fr-CA", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("fr-CA");
                else
                    return CultureInfo.CreateSpecificCulture("fr-FR");
            }
            //"it"
            if (cultureIn.TwoLetterISOLanguageName.Equals("it", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("it");
            }
            //"ja"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ja", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("ja-JP");
                else
                    return CultureInfo.CreateSpecificCulture("ja");
            }

            //"ko"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("ko-KR", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("ko-KR");
                else
                    return CultureInfo.CreateSpecificCulture("ko");
            }
            //"pl"
            if (cultureIn.TwoLetterISOLanguageName.Equals("pl", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("pl");
            }
            //"pt"
            if (cultureIn.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("pt-BR", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("pt-BR");
                else
                    return CultureInfo.CreateSpecificCulture("pt-PT");
            }
            //"ru"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("ru");
            }

            //"tr"
            if (cultureIn.TwoLetterISOLanguageName.Equals("tr", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("ru");
            }

            //"uk"
            if (cultureIn.TwoLetterISOLanguageName.Equals("uk", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("uk");
            }

            //"zh"
            if (cultureIn.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                switch (cultureIn.Name)
                {
                    case "zh-CN":
                        return CultureInfo.CreateSpecificCulture("zh-CN");
                    case "zh-TW":
                        return CultureInfo.CreateSpecificCulture("zh-TW");

                    case "zh-Hans":
                    case "zh-SG":
                        return CultureInfo.CreateSpecificCulture("zh-Hans");
                    default:
                        return CultureInfo.CreateSpecificCulture("zh-Hant");
                }
            } //
            //Otherwise, return the input cultureInfo
            return cultureIn;
        }
    }
}
