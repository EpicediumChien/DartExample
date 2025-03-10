using nsLogWriter;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace GenDdpmSplashImages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel vm = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;

            LogWriter.LogPathName = App.GetAppDir(true) + "GenDdpmSplashImages.log";
            LogWriter.Enabled = true;
            LogWriter.LogLine("------------------------------------------------------");

            DirectoryInfo di = Directory.GetParent(App.GetAppDir(false));
            string solutionDir = di.FullName;
            LogWriter.LogLine($"AppDir=[{App.GetAppDir(false)}], SolutionDir=[{solutionDir}]");

            ReadSharedAssemblyInfo(solutionDir);

            vm.Year = App.Year;
            vm.Build = App.Build;
            vm.Color = App.Color;
            vm.Size = App.Size;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogWriter.LogLine($"Creating Images, with parameters: Build=[{vm.Build}], Version=[{vm.Version}], Year=[{vm.Year}], Size=[{vm.Size}], Color=[{vm.Color}]");

            Dispatcher.Invoke(() =>
            {
                if (vm.Color.Equals("dark", StringComparison.OrdinalIgnoreCase))
                {
                    vm.ImageFilePath = "assets/splash4k-roundx4.png";
                    vm.Dell_Sign_color = new SolidColorBrush(Colors.White);
                    if (vm.Size.Equals("4k", StringComparison.OrdinalIgnoreCase))
                    {                    
                        vm.ChangeTo4KImage();
                        //create_darkmode_image4k();
                    }
                    else //normal size
                    {                        
                        //create_darkmode_image();
                    }
                }
                else //light mode
                {
                    vm.Dell_Sign_color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E0E0E"));
                    vm.ImageFilePath = "assets/splash4k-roundx4_light.png";  
                    Thread.Sleep(500);
                    if (vm.Size.Equals("4k", StringComparison.OrdinalIgnoreCase))
                    {
                        vm.ChangeTo4KImage();
                        //create_lightmode_image4k();
                    }
                    else //normal size
                    {
                        //create_lightmode_image();
                    }
                }
            });
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
                Thread.Sleep(500);
            };
            bw.RunWorkerCompleted += delegate
            {
                Thread.Sleep(1000);
                if (vm.Color.Equals("dark", StringComparison.OrdinalIgnoreCase))
                {
                    if (vm.Size.Equals("4k", StringComparison.OrdinalIgnoreCase))
                    {
                        create_darkmode_image4k();
                    }
                    else //normal size
                    {
                        create_darkmode_image();
                    }
                }
                else //light mode
                {
                    if (vm.Size.Equals("4k", StringComparison.OrdinalIgnoreCase))
                    {
                        create_lightmode_image4k();
                    }
                    else //normal size
                    {
                        create_lightmode_image();
                    }
                }
                Close();
            };
            bw.RunWorkerAsync();
        }

        private void create_darkmode_image()
        {
            //
            // Dark mode image normal
            //
            BitmapSource image = CreateBitmapSource();
            if (image != null)
            {
                string pathName = App.GetAppDir() + "splash-round.png";
                SaveBitmapSourceToPngFile(image, pathName);
                LogWriter.LogLine($"Image=[{pathName}], Created OK.");
            }
            else
            {
                LogWriter.LogLine($"Image=[splash-round.png], Created FAILED.");
            }
        }

        private void create_darkmode_image4k()
        {
            //
            // Dark mode image 4k
            //
            BitmapSource image = CreateBitmapSource();
            if (image != null)
            {
                string pathName = App.GetAppDir() + "splash4k-round.png";
                SaveBitmapSourceToPngFile(image, pathName);
                LogWriter.LogLine($"Image=[{pathName}], Created OK.");
            }
            else
            {
                LogWriter.LogLine($"Image=[splash4k-round.png], Created FAILED.");
            }
        }

        private void create_lightmode_image()
        { 
            BitmapSource image = CreateBitmapSource();
            if (image != null)
            {
                string pathName = App.GetAppDir() + "splash-round_light.png";
                Thread.Sleep(100);
                SaveBitmapSourceToPngFile(image, pathName);
                LogWriter.LogLine($"Image=[{pathName}], Created OK.");
            }
            else
            {
                LogWriter.LogLine($"Image=[splash-round_light.png], Created FAILED.");
            }
        }

        private void create_lightmode_image4k()
        {             
            BitmapSource image = CreateBitmapSource();
            if (image != null)
            {
                string pathName = App.GetAppDir() + "splash4k-round_light.png";
                Thread.Sleep(100);
                SaveBitmapSourceToPngFile(image, pathName);
                LogWriter.LogLine($"Image=[{pathName}], Created OK.");
            }
            else
            {
                LogWriter.LogLine($"Image=[splash4k-round_light.png], Created FAILED.");
            }
        }
        
        /// <summary>
        /// To create a BitmatSouce from current SplitCtrl. The return BitmapSource can be used to
        /// 1) Display an Image on GUI, 2) Save as a .PNG file
        /// </summary>
        /// <returns></returns>
        public BitmapSource CreateBitmapSource()
        {
            double pxWidth = vm.ImageWidth;// splash.ActualWidth + 1;
            double pxHeight = vm.ImageHeight;// splash.ActualHeight + 1;

            if ((pxWidth <= 0) && (pxHeight <= 0))
                return null;

            splash.Measure(new Size(pxWidth, pxHeight));
            splash.Arrange(new Rect(new Size(pxWidth, pxHeight)));

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)pxWidth, (int)pxHeight,
                96d, 96d, System.Windows.Media.PixelFormats.Default);

            rtb.Render(splash);
            return rtb;
        }

        /*public BitmapSource CreateBitmapSource_light()
        {           
            double pxWidth = vm.ImageWidth;// splash.ActualWidth + 1;
            double pxHeight = vm.ImageHeight;// splash.ActualHeight + 1;

            if ((pxWidth <= 0) && (pxHeight <= 0))
                return null;

            splash.Measure(new Size(pxWidth, pxHeight));
            splash.Arrange(new Rect(new Size(pxWidth, pxHeight)));

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)pxWidth, (int)pxHeight,
                96d, 96d, System.Windows.Media.PixelFormats.Default);

            rtb.Render(splash);
            Thread.Sleep(500);
            return rtb;
        }*/

        private void SaveBitmapSourceToPngFile(BitmapSource image, string pathName)
        {
            using (var fileStream = new FileStream(pathName, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

        private bool ReadSharedAssemblyInfo(string solutionDir)
        {
            string pathName = System.IO.Path.Combine(solutionDir, "SharedAssemblyInfo.cs");
            LogWriter.LogLine($"Read from [{pathName}]");
            string fileContent = "";

            try
            {
                fileContent = File.ReadAllText(pathName);
            }
            catch (Exception e1)
            {
                LogWriter.LogLine($"Read file exception: {e1.Message}");
                return false;
            }

            //1 Try to read Version
            //  Expected input:
            //    [assembly: System.Reflection.AssemblyFileVersion("2.0.0.24")]
            string signatureAssemblyFileVersion = "AssemblyFileVersion(\"";
            int idxAssemblyFileVersion = fileContent.IndexOf(signatureAssemblyFileVersion);

            if (idxAssemblyFileVersion > 0)
            {
                LogWriter.LogLine($"Found [{signatureAssemblyFileVersion}] at Index=[{idxAssemblyFileVersion}]");
                int idxStartVersion = idxAssemblyFileVersion + signatureAssemblyFileVersion.Length;
                //Find the next " char
                int idxEndAssemblyFileVersion = fileContent.IndexOf("\"", idxStartVersion);
                if (idxEndAssemblyFileVersion > 0)
                {
                    int lengthVersion = idxEndAssemblyFileVersion - idxStartVersion;
                    string build = fileContent.Substring(idxStartVersion, lengthVersion);
                    vm.Build = build;

                    LogWriter.LogLine($"Extract Build=[{vm.Build}]");

                    string version = build;
                    Version ver;
                    if (Version.TryParse(version, out ver))
                    {
                        version = $"{ver.Major}.{ver.Minor}.{ver.Build}";
                    }

                    vm.Version = version;
                    LogWriter.LogLine($"Output Version=[{vm.Version}]");
                }
                else
                {
                    LogWriter.LogLine($"Cannot found [{signatureAssemblyFileVersion}]");
                }
            }

            return true;
        }
    }
}