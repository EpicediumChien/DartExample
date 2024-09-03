using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Views;
using Dell.Client.Framework.UX.WPF;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static DDPM.UI.Common.Views.DDPMMsgBox;

namespace DDPM.UI.Common
{
    public static class DdpmCommonHelper
    {
        /// <summary>
        /// Create an ImageSource object, which load from DDPM.Common.Resources
        /// </summary>
        /// <param name="resourceName">The filename of the image file, for example, "Resources/dell.png"</param>
        /// <returns></returns>
        public static ImageSource GetImageSourceFromCommonResource(string resourceName, string assemblyName = "")
        {
            if (String.IsNullOrWhiteSpace(assemblyName))
                assemblyName = "DDPM.UI.Common";

            try
            {
                Uri oUri = new Uri("pack://application:,,,/" + assemblyName + ";component/" + resourceName, UriKind.RelativeOrAbsolute);
                return BitmapFrame.Create(oUri);
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        //DdpmHomePlugin will set this value
        public static IConsole? MyConsole { get; set; }

        public static IDeviceManagerSA? DeviceManagerSA { get; set; }
        public static DDPMSettings? Settings_Cache { get; set; }

        public static IModuleOwner? ModuleOwner { get; set; }

        public static bool DDPMMesssageBox(string title, string text)
        {
            //MessageBoxResult result = System.Windows.MessageBox.Show(text, "Confirmation", MessageBoxButton.YesNo);
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, null);
            msgBox.ShowDialog();

            //if (result == MessageBoxResult.Yes)
            if (msgBox.result == DDPMMsgBox_btn_result.left)
                return true;
            else
                return false;
        }

        public static void DDPMPureMesssageBox(string title, string text, bool IsCloseButton, Window Owner)
        {
            DDPMMsgBox msgBox = new DDPMMsgBox(title, text, IsCloseButton, Owner);
            msgBox.ShowDialog();
        }

        public static bool IsMainWindowAtPrimaryScreen = true;

        /// <summary>
        ///Parsing hex value blank separated string to a WORD array
        ///Support format:
        ///1 All Bytes: "02 04 05 08 10 12"
        ///2 All Words: "0002 0004 0105 0208 1006 AE12"
        ///3 Mix: "02 0208 04 05 AE12"
        ///4 Multiple space chars: "  02 04  05     08 10 1006    12"
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns>
        /// null : Invalid format in inStr
        /// empty : no any token found
        /// </returns>
        public static UInt16[] ParsingHexStringToWords(string inStr)
        {
            //1 Split into tokens with white space
            string[] tokens = inStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens == null || tokens.Length == 0)
                return new UInt16[0];

            List<UInt16> words = new List<UInt16>();
            //2 for each token will convert to UInt16 integer value
            foreach (string tok in tokens)
            {
                UInt16 wValue;
                if (!UInt16.TryParse(tok, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out wValue))
                {
                    return null;
                }
                words.Add(wValue);
            }
            return words.ToArray();
        }
    }
}