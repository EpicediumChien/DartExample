using DDPM.SA.Common;
using DDPM.UI.Resources;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// Interaction logic for ThirdPartyLicenses.xaml
    /// </summary>
    public partial class ThirdPartyLicenses : Window, INotifyPropertyChanged
    {
        private ResourceManager resManager = ThirdPartyLicense.ResourceManager;
        public ObservableCollection<UI_ThirdPartyLicenses> ThirdPartyLicensesList { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ThirdPartyLicenses()
        {
            InitializeComponent();
            DataContext = this;

        }
        private void UXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThirdPartyLicensesList = new ObservableCollection<UI_ThirdPartyLicenses>();
            ResourceSet resourceSet = resManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            int resourceCount = 0;
            if (resourceSet != null)
            {
                foreach (DictionaryEntry entry in resourceSet)
                {
                    resourceCount++;
                }
                for (int i = 0; i < resourceCount / 2; i++)
                {
                    ThirdPartyLicensesList.Add(new UI_ThirdPartyLicenses()
                    {
                        Title = resManager.GetString($"Title{i + 1}"),
                        Content = resManager.GetString($"Content{i + 1}")
                    });

                    //PIMS-313975
                    TextToCopy += Title;
                    TextToCopy += (System.Environment.NewLine + System.Environment.NewLine + Content + System.Environment.NewLine);
                }
            }
            OnPropertyChanged("TextToCopy");
            OnPropertyChanged("ThirdPartyLicensesList");
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private string _textToCopy = string.Empty;
        public string TextToCopy
        {
            get => _textToCopy;
            set
            {
                _textToCopy = value;
                //OnPropertyChanged("TextToCopy");
            }
        }
    }
    public class UI_ThirdPartyLicenses
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public UI_ThirdPartyLicenses()
        {
            Title = string.Empty;
            Content = string.Empty;
        }
    }
}
