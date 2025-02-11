using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Resources.Helper
{
    public class LangHelper : INotifyPropertyChanged
    {
        private readonly ResourceManager _resourceManager;
        private static readonly Lazy<LangHelper> _lazy = new Lazy<LangHelper>(() => new LangHelper());
        public static LangHelper Instance => _lazy.Value;

        public event PropertyChangedEventHandler PropertyChanged;
        public static CultureInfo? UserMappedCultureInfo;
        public LangHelper()
        {
            //Get the resources of the Lang of the Resources in this namespace, which can be modified.
            _resourceManager = new ResourceManager("DDPM.SA.Resources.Resources", typeof(LangHelper).Assembly);
        }

        /// <summary>
        /// Pass in the cursor of the string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

#if FALSE
                //resManager.GetString(key, CultureInfo.InstalledUICulture) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
                CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("fr-FR");
                string str = _resourceManager.GetString(name, cultureInfo) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";
                //string str = _resourceManager.GetString(name, CultureInfo.InstalledUICulture) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";
                return System.Text.RegularExpressions.Regex.Unescape(str);
#else
                //resManager.GetString(key, CultureInfo.InstalledUICulture) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
                //CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("ru");
                //string str = _resourceManager.GetString(name, cultureInfo) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";

                //Robert_Lin 2025-1-22 PIMS-331191 With DDPM installed, observe language in DDPM UI not change for other langauges of Other countries
                //Add a CultureInfoMap to convert (mapped) CultureInfo.CurrentUICulture to the supported cultureInfo of DDPM
                //OLD:
                //string str = _resourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";
                //NEW:
                //Bruce 02/11 Fix PIMS-344808 modify the language used by System to be the same as that captured by user.
                string str;
                if (UserMappedCultureInfo != null)
                {
                    str = _resourceManager.GetString(name, UserMappedCultureInfo) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";
                }
                else
                {
                    str = _resourceManager.GetString(name, DdpmCultureMap.MappedCultureInfo) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";
                }
                return System.Text.RegularExpressions.Regex.Unescape(str);
#endif
            }
        }

        public void ChangeLanguage(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("item[]"));  //A collection of strings, corresponding to the values of the resources
        }
        public static CultureInfo GetLanguage()
        {
            //Bruce 02/11 Fix PIMS-344808 modify the language used by System to be the same as that captured by user.
            return DdpmCultureMap.MappedCultureInfo;
        }
    }
}
