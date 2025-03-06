using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Resources;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
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
using System.Windows.Forms;
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
    public partial class ThirdPartyLicenses : Window //, INotifyPropertyChanged
    {
        private ResourceManager resManager = ThirdPartyLicense.ResourceManager;
        private ResourceManager resManager_NKVM = ThirdPartyLicense_NKVM.ResourceManager;
        private ILog? _log;

        //public ObservableCollection<UI_ThirdPartyLicenses> ThirdPartyLicensesList { get; set; }
        public List<UI_ThirdPartyLicenses> ThirdPartyLicensesList { get; set; }
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        public ThirdPartyLicenses()
        {
            _log = SettingsPlugin.PluginIoc?.GetService<ILog>();
            _log?.Info("ThirdPartyLicenses initialize start");
            InitializeComponent();
            DataContext = this;
            _log?.Info("ThirdPartyLicenses initialize done");

        }
        private void UXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _log?.Info("ThirdPartyLicenses UXWindow_Loaded start");
            //ThirdPartyLicensesList = new ObservableCollection<UI_ThirdPartyLicenses>();
            ThirdPartyLicensesList = new();
            ResourceSet resourceSet = resManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            ResourceSet resource_NKVMSet = resManager_NKVM.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            int resourceCount = 0;
            if (resourceSet != null)
            {
                foreach (DictionaryEntry entry in resourceSet)
                {
                    resourceCount++;
                }
                _log?.Info($"ThirdPartyLicenses resourceCount : {resourceCount}");
                for (int i = 0; i < resourceCount / 2; i++)
                {
                    ThirdPartyLicensesList.Add(new UI_ThirdPartyLicenses()
                    {
                        Title = resManager.GetString($"Title{i + 1}"),
                        Content = resManager.GetString($"Content{i + 1}")
                    });

                    //PIMS-313975
                    //TextToCopy += Title;
                    //TextToCopy += (System.Environment.NewLine + System.Environment.NewLine + Content + System.Environment.NewLine);
                }
            }
            resourceCount = 0;
            if (resource_NKVMSet != null)
            {
                foreach (DictionaryEntry entry in resource_NKVMSet)
                {
                    resourceCount++;
                }
                _log?.Info($"ThirdPartyLicenses resourceCount : {resourceCount}");
                for (int i = 0; i < resourceCount / 2; i++)
                {
                    ThirdPartyLicensesList.Add(new UI_ThirdPartyLicenses()
                    {
                        Title = resManager_NKVM.GetString($"Title{i + 1}"),
                        Content = resManager_NKVM.GetString($"Content{i + 1}")
                    });

                    //PIMS-313975
                    //TextToCopy += Title;
                    //TextToCopy += (System.Environment.NewLine + System.Environment.NewLine + Content + System.Environment.NewLine);
                }
            }
            _log?.Info($"ThirdPartyLicenses ThirdPartyLicensesList.Count : {ThirdPartyLicensesList.Count}");
            //OnPropertyChanged("TextToCopy");
            //OnPropertyChanged("ThirdPartyLicensesList");

            FlowDocument doc = new();
            Paragraph paragraph = new()
            {
                LineHeight = 24
            };
            foreach (var item in ThirdPartyLicensesList)
            {
                Run run1 = new($"{item.Title}")
                {
                    FontSize = 20,
                    FontWeight = FontWeights.Bold
                };
                Run run2 = new($"\n{item.Content}\n")
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Normal
                };
                paragraph.Inlines.Add(run1);
                paragraph.Inlines.Add(run2);
            }
            doc.Blocks.Add(paragraph);
            txtThirdPartyLicense.Document = doc;
            txtThirdPartyLicense.Foreground = DdpmCommonHelper.isDarkMode() ? new SolidColorBrush(Color.FromRgb(0xB6, 0xB6, 0xB6)) : Brushes.Black;
            _log?.Info("ThirdPartyLicenses UXWindow_Loaded done");
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        //private string _textToCopy = string.Empty;
        //public string TextToCopy
        //{
        //    get => _textToCopy;
        //    set
        //    {
        //        _textToCopy = value;
        //        //OnPropertyChanged("TextToCopy");
        //    }
        //}

        private void Window_Deactivated(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                _log?.Error("Window Deactivated already", ex);
            }
        }

        private void txtThirdPartyLicense_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                txtThirdPartyLicense.SelectAll();
                e.Handled = true;
            }
        }

        private void txtThirdPartyLicense_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is System.Windows.Controls.RichTextBox rtb)
            {
                var menuItem = rtb.ContextMenu.Items[0] as MenuItem;
                menuItem!.IsEnabled = !rtb.Selection.IsEmpty;
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
