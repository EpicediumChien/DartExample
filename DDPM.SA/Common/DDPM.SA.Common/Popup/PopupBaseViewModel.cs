using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Resources.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Threading;

namespace DDPM.SA.Common.Popup
{
    public class PopupBaseViewModel : ObservableObject
    {
        private string header;
        public string Header
        {
            get => header;
            set
            {
                if (header != value)
                {
                    header = value;
                    OnPropertyChanged(nameof(Header));
                }
            }
        }

        private string subHeader;
        public string SubHeader
        {
            get => subHeader;
            set
            {
                if (subHeader != value)
                {
                    subHeader = value;
                    OnPropertyChanged(nameof(SubHeader));
                }
            }
        }
        bool isInstalling;
        public void UpdateContent(string HeaderText, UpdateProgressInfo e)
        {
            Header = HeaderText;
            if (e != null)
            {
                Header = $"{HeaderText} {e.Model}";
                if (e.ProcessName.Equals("Installing"))
                {
                    isInstalling = true;
                    SubHeader = $"{LangHelper.Instance["Processing"]}... {(int)e.ProcessProgress}%";
                }
                else if (e.ProcessName.Equals("Downloading"))
                {
                    isInstalling = false;
                    SubHeader = $"{LangHelper.Instance["Downloading_and_installing"]}... {(int)e.ProcessProgress}%";
                }
                else if (e.ProcessName.Equals("M1"))
                {
                    isInstalling = false;
                    SubHeader = LangHelper.Instance["M1_Please_double_click_mouse_left_button_to_start_firmware_update"];
                }
                else if (e.ProcessName.Equals("M2"))
                {
                    isInstalling = false;
                    SubHeader = LangHelper.Instance["M2_Please_press_key_on_keyboard_to_start_firmware_update"];
                }
                else if (e.ProcessName.Equals("Timeout") && !isInstalling)
                {
                    SubHeader = $"{LangHelper.Instance["Unable_to_detect_target_device"]}… {(int)e.ProcessProgress}s";
                }
            }
        }
    }
}
