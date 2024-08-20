using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace DDPM.UI.Resources.Helper
{
    public class LangHelper : INotifyPropertyChanged
    {
        private readonly ResourceManager _resourceManager;
        private static readonly Lazy<LangHelper> _lazy = new Lazy<LangHelper>(() => new LangHelper());
        public static LangHelper Instance => _lazy.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        public LangHelper()
        {
            //Get the resources of the Lang of the Resources in this namespace, which can be modified.
            _resourceManager = new ResourceManager("DDPM.UI.Resources.Resources", typeof(LangHelper).Assembly);
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
                //resManager.GetString(key, CultureInfo.InstalledUICulture) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
                CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pl-PL");
                string str = _resourceManager.GetString(name, cultureInfo) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";
               // string str = _resourceManager.GetString(name, CultureInfo.InstalledUICulture) ?? _resourceManager.GetString(name, CultureInfo.InvariantCulture) ?? "";
                return System.Text.RegularExpressions.Regex.Unescape(str);
            }
        }

        public void ChangeLanguage(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("item[]"));  //A collection of strings, corresponding to the values of the resources
        }
    }
}