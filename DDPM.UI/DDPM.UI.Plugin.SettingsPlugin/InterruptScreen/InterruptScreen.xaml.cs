using DDPM.SA.Common;
using DDPM.UI.Resources;
using Dell.Client.Framework.UX.WPF.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
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

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// Interaction logic for InterruptScreen.xaml
    /// </summary>
    public partial class InterruptScreen : Window, INotifyPropertyChanged
    {
        public BitmapImage BackgroundImage { get; set; }
        public string NewDeviceName { get; set; }
        public ObservableCollection<UI_NewSupportedDevices> NewSupportedDevicesCollection { get; set; }
        public ObservableCollection<UI_NewFeatures> NewFeaturesList { get; set; }
        public ObservableCollection<UI_BugFixesContent> BugFixesList { get; set; }
        private int _currentIndex = 0;
        private DispatcherTimer _timer;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public InterruptScreen()
        {
            InitializeComponent();
            DataContext = this;
            //_InterruptScreenRoot = interruptScreenRoot;
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
            string path = @"D:\Update\FW_Update_Info\FW-UPDATE-LOCAL-FOR-ODM\DPM-Updates\WebServer\appRoot\test\ddpm\AppUpdates.json";
            string jsonString = File.ReadAllText(path);
            InterruptScreenRoot myDeserializedClass = JsonConvert.DeserializeObject<InterruptScreenRoot>(jsonString);
            NewSupportedDevices.Visibility = Visibility.Collapsed;
            NewFeatures.Visibility = Visibility.Collapsed;
            BugFixes.Visibility = Visibility.Collapsed;
            foreach (FeaturesList featuresList in myDeserializedClass.featuresList)
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
                                Title = featuresList.content.productLabel.source,
                                BackgroundImage = LoadLocalImage($@"D:\Update\FW_Update_Info\FW-UPDATE-LOCAL-FOR-ODM\DPM-Updates\WebServer\appRoot\test\ddpm\{featuresList.content.imageUrl}"),
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
                                    Content = detailsList.source
                                });
                            }
                            NewFeaturesList.Add(new UI_NewFeatures()
                            {
                                NewFeatures_Image = LoadLocalImage($@""),
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
                                BugFixesContent = featuresList.content.bugDescription.source
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
            OnPropertyChanged("BackgroundImage");
            OnPropertyChanged("NewDeviceName");
            OnPropertyChanged("NewSupportedDevicesCollection");
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

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void NextItem()
        {
            _currentIndex = (_currentIndex + 1) % NewSupportedDevicesCollection.Count;
            if (NewSupportedDevicesCollection.Count> _currentIndex)
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
}
