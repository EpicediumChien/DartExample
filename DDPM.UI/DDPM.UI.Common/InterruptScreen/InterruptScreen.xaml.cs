using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Resources;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.UX.WPF.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace DDPM.UI.Common
{
    /// <summary>
    /// Interaction logic for InterruptScreen.xaml
    /// </summary>
    public partial class InterruptScreen : Window, INotifyPropertyChanged
    {
        public BitmapImage BackgroundImage { get; set; }
        public string NewDeviceName { get; set; }
        public string Available_Title { get; set; }
        public ObservableCollection<UI_NewSupportedDevices> NewSupportedDevicesCollection { get; set; }
        public ObservableCollection<UI_NewFeatures> NewFeaturesList { get; set; }
        public ObservableCollection<UI_BugFixesContent> BugFixesList { get; set; }
        private int _currentIndex = 0;
        private DispatcherTimer _timer;
        InterruptScreenRoot _InterruptScreenRoot;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public InterruptScreen(string versionNumber,InterruptScreenRoot interruptScreenRoot)
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            DataContext = this;
            if (interruptScreenRoot == null)
            {
                this.Close();
            }
            Available_Title = $"{LangHelper.Instance["Update_Available_Version"]} {versionNumber}";
            _InterruptScreenRoot = interruptScreenRoot;
            NewSupportedDevicesCollection = new ObservableCollection<UI_NewSupportedDevices>();
            NewFeaturesList = new ObservableCollection<UI_NewFeatures>();
            BugFixesList = new ObservableCollection<UI_BugFixesContent>();
            LodaData();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _timer.Tick += (sender, e) => NextItem();
            _timer.Start();
        }
        void LodaData()
        {
            if (_InterruptScreenRoot != null)
            {
                NewSupportedDevices.Visibility = Visibility.Collapsed;
                NewFeatures.Visibility = Visibility.Collapsed;
                BugFixes.Visibility = Visibility.Collapsed;
                foreach (FeaturesList featuresList in _InterruptScreenRoot.featuresList)
                {
                    switch (featuresList.categoryId)
                    {
                        case 1:
                            if (featuresList.content.productLabel != null)
                            {
                                NewSupportedDevices.Visibility = Visibility.Visible;
                                Console.WriteLine($"{featuresList.content.productLabel.source}");
                                NewSupportedDevicesCollection.Add(new UI_NewSupportedDevices()
                                {
                                    Title = GetTranslation(featuresList.content.productLabel),
                                    BackgroundImage = ConvertByteArrayToBitmapImage(featuresList.content.image),
                                });
                            }
                            break;
                        case 2:
                            if (featuresList.content.detailsList != null)
                            {
                                NewFeatures.Visibility = Visibility.Visible;
                                ObservableCollection<UI_NewFeaturesContent> temp = new ObservableCollection<UI_NewFeaturesContent>();
                                foreach (DetailsList detailsList in featuresList.content.detailsList)
                                {
                                    temp.Add(new UI_NewFeaturesContent()
                                    {
                                        Content = GetTranslation(detailsList)
                                    });
                                }
                                NewFeaturesList.Add(new UI_NewFeatures()
                                {
                                    NewFeatures_Image = ConvertByteArrayToBitmapImage(featuresList.content.image),
                                    NewSupportedDevicesCollection = temp
                                });
                            }
                            break;
                        case 3:
                            if (featuresList.content.bugDescription != null)
                            {
                                BugFixes.Visibility = Visibility.Visible;
                                BugFixesList.Add(new UI_BugFixesContent()
                                {
                                    BugFixesContent = GetTranslation(featuresList.content.bugDescription)
                                });
                            }
                            break;
                    }
                }
                if (NewSupportedDevicesCollection.Count > 0)
                {
                    BackgroundImage = NewSupportedDevicesCollection[_currentIndex].BackgroundImage;
                    NewDeviceName = NewSupportedDevicesCollection[_currentIndex].Title;
                }
            }
            OnPropertyChanged("BackgroundImage");
            OnPropertyChanged("NewDeviceName");
            OnPropertyChanged("NewSupportedDevicesCollection");
        }
        private static BitmapImage ConvertByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bitmapImage = new BitmapImage();

            if (byteArray.Length > 0)
            {
                using (MemoryStream memoryStream = new MemoryStream(byteArray))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
            }
            return bitmapImage;
        }

        private BitmapImage LoadLocalImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.EndInit();
                    /*
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] imageBytes = client.GetByteArrayAsync(path).Result;
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = ms;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                        }
                    }
                    */
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                }
            }
            return bitmap;
        }
        private string GetTranslation(object o)
        {
            string ret = "";
            ProductLabel productLabel = o as ProductLabel;
            DetailsList detailsList = o as DetailsList;
            BugDescription bugDescription = o as BugDescription;
            if (productLabel != null)
            {
                ret = productLabel.source;
                if (productLabel.translations != null)
                {
                    string s = GetTranslationByLanguage(productLabel.translations, CultureInfo.CurrentUICulture);
                    if (!string.IsNullOrEmpty(s))
                    {
                        ret = s;
                    }
                }
            }
            if (detailsList != null)
            {
                ret = detailsList.source;
                if (detailsList.translations != null)
                {
                    string s = GetTranslationByLanguage(detailsList.translations, CultureInfo.CurrentUICulture);
                    if (!string.IsNullOrEmpty(s))
                    {
                        ret = s;
                    }
                }
            }
            if (bugDescription != null)
            {
                ret = bugDescription.source;
                if (bugDescription.translations != null)
                {
                    string s = GetTranslationByLanguage(bugDescription.translations, CultureInfo.CurrentUICulture);
                    if (!string.IsNullOrEmpty(s))
                    {
                        ret = s;
                    }
                }
            }
            return ret;
        }
        public static string GetTranslationByLanguage(Translations translations, CultureInfo cultureIn)
        {
            //"ar": All convert to "ar-SA"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase))
            {
                return translations.ar;
            }
            //"de": All convert to "de-DE"
            if (cultureIn.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase))
            {
                return translations.de;
            }
            //"es"
            if (cultureIn.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase))
            {
                return translations.es;
            }// if "es"
            //"fr"
            if (cultureIn.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("fr-CA", StringComparison.OrdinalIgnoreCase))
                    return translations.fr_CA;
                else
                    return translations.fr;
            }
            //"it"
            if (cultureIn.TwoLetterISOLanguageName.Equals("it", StringComparison.OrdinalIgnoreCase))
            {
                return translations.it;
            }
            //"ja"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ja", StringComparison.OrdinalIgnoreCase))
            {
                return translations.ja;

            }

            //"ko"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase))
            {
                return translations.ko;

            }
            //"pl"
            if (cultureIn.TwoLetterISOLanguageName.Equals("pl", StringComparison.OrdinalIgnoreCase))
            {
                return translations.pl;
            }
            //"pt"
            if (cultureIn.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("pt-BR", StringComparison.OrdinalIgnoreCase))
                    return translations.pt_BR;
            }
            //"ru"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase))
            {
                return translations.ru;
            }

            //"tr"
            if (cultureIn.TwoLetterISOLanguageName.Equals("tr", StringComparison.OrdinalIgnoreCase))
            {
                return translations.tr;
            }

            //"uk"
            if (cultureIn.TwoLetterISOLanguageName.Equals("uk", StringComparison.OrdinalIgnoreCase))
            {
                return translations.uk;
            }

            //"zh"
            if (cultureIn.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                switch (cultureIn.Name)
                {

                    case "zh-Hans-CN":
                    case "zh-CN":
                        return translations.zh;
                    case "zh-TW":
                        return translations.zh_TW;

                    case "zh-Hans":
                    case "zh-SG":
                        return translations.zh;
                    default:
                        return translations.zh_TW;
                }
            } //
            //Otherwise, return the input cultureInfo
            return "";
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void NextItem()
        {
            _currentIndex = (_currentIndex + 1) % NewSupportedDevicesCollection.Count;
            if (NewSupportedDevicesCollection.Count > _currentIndex)
            {
                BackgroundImage = NewSupportedDevicesCollection[_currentIndex].BackgroundImage;
                NewDeviceName = NewSupportedDevicesCollection[_currentIndex].Title;
            }
            OnPropertyChanged("BackgroundImage");
            OnPropertyChanged("NewDeviceName");
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Tick -= (sender, e) => NextItem();
                _timer.Stop();
                _timer = null;
            }
        }

        private void UXButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void LearnMoreForSoftware_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.dell.com/support/home";
            try
            {
                DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                    null,
                    new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
            }
            catch
            {
            }
        }

        private void LearnMoreForFirmware_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.dell.com/support/home";
            try
            {
                DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                    null,
                    new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
            }
            catch
            {
            }
        }
    }
    public class UI_NewSupportedDevices
    {
        public string Title { get; set; }
        public string Content_1 { get; set; }
        public string Content_2 { get; set; }
        public BitmapImage BackgroundImage { get; set; }
        public UI_NewSupportedDevices()
        {
            Title = string.Empty;
            Content_1 = string.Empty;
            Content_2 = string.Empty;
        }
    }
    public class UI_NewFeatures
    {
        public BitmapImage NewFeatures_Image { get; set; }
        public ObservableCollection<UI_NewFeaturesContent> NewSupportedDevicesCollection { get; set; }
        public UI_NewFeatures()
        {
            NewSupportedDevicesCollection = new ObservableCollection<UI_NewFeaturesContent>();
        }
    }
    public class UI_NewFeaturesContent
    {
        public string Content { get; set; }
        public UI_NewFeaturesContent()
        {
            Content = string.Empty;
        }
    }
    public class UI_BugFixesContent
    {
        public string BugFixesContent { get; set; }
        public UI_BugFixesContent()
        {
            BugFixesContent = string.Empty;
        }
    }
}
