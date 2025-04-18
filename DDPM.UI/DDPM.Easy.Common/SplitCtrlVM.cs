using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace DDPM.Easy.Common
{
    public class SplitCtrlVM : ObservableObject, IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion Private members

        #region SplitMode

        private eSplitModes splitMode = eSplitModes.Icon;

        public eSplitModes SplitMode
        {
            get => splitMode;
            set
            {
                SetProperty(ref splitMode, value);
                OnPropertyChanged("IsEditMode");
                OnPropertyChanged("IsWorkMode");
                OnPropertyChanged("IsIconMode");
                OnPropertyChanged("IsAwsMode");
            }
        }

        //Wrokaround, until I can databinding, trigger with SplitMode directly
        public bool IsEditMode => SplitMode == eSplitModes.Edit;

        public bool IsWorkMode => SplitMode == eSplitModes.Work;
        public bool IsIconMode => SplitMode == eSplitModes.Icon;
        public bool IsAwsMode => SplitMode == eSplitModes.AWS;

        #endregion SplitMode

        #region IsEditable (inside EditWindow or WorkWidnow)

        private bool isEditable = false;

        public bool IsEditable
        {
            get { return isEditable; }
            set { isEditable = value; OnPropertyChanged("IsEditable"); }
        }

        #endregion IsEditable (inside EditWindow or WorkWidnow)

        #region Border Thickness

        private double thickBorder = 8.00;

        public double ThickBorder
        {
            get { return thickBorder; }
            set
            {
                thickBorder = value;
                OnPropertyChanged("ThickBorder");
                OnPropertyChanged("ThicknessBorder");
            }
        }

        public Thickness ThicknessBorder
        {
            get { return new Thickness(ThickBorder); }
        }

        #endregion Border Thickness

        #region GridSplitter Thickness

        private double thickSplitter = 8.00;

        public double ThickSplitter
        {
            get { return thickSplitter; }
            set { thickSplitter = value; OnPropertyChanged("ThickSplitter"); }
        }

        #endregion GridSplitter Thickness

        #region Screen Orientation

        private bool isVertical = false;

        public bool IsVertical
        {
            get { return isVertical; }
            set { isVertical = value; OnPropertyChanged("IsVertical"); }
        }

        #endregion Screen Orientation

        #region Custom Settings

        private ObservableCollection<GridLength> settings = null;

        public ObservableCollection<GridLength> Settings
        {
            get { return settings; }
            set
            {
                settings = value;
                OnPropertyChanged("Settings");
                OnPropertyChanged("Settings_Double");
                OnPropertyChanged("Settings_String");
            }
        }

        public List<double> Settings_Double
        {
            get { return GridLength_To_Double(Settings); }
        }

        //Format: "[1,2,1,0.8,1,4.52]"
        public string Settings_String
        {
            get { return Double_To_String(Settings_Double); }
        }

        //GridLength[] => double[]
        public static List<double> GridLength_To_Double(ObservableCollection<GridLength> settings)
        {
            if (settings == null)
                return null;
            List<double> listOut = new List<double>();
            foreach (GridLength g in settings)
            {
                listOut.Add(g.Value);
            }
            return listOut;
        }

        //string("1,2,1.33,2,1") => double[]
        public static List<double> String_To_Double(string strSettings)
        {
            try
            {
                return Array.ConvertAll(strSettings.Split(','), Double.Parse).ToList<double>();
            }
            catch (Exception e1)
            {
                return new List<double>();
            }
        }

        //double[] => GridLength[]
        public static ObservableCollection<GridLength> Double_To_GridLength(List<double> settings)
        {
            if (settings == null)
                return null;
            ObservableCollection<GridLength> listOut = new ObservableCollection<GridLength>();
            foreach (double v in settings)
            {
                listOut.Add(new GridLength(v, GridUnitType.Star));
            }
            return listOut;
        }

        //double[] to string, format: [1,2,1,0.8,1,4.52]
        public static string Double_To_String(List<double> settings)
        {
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (double v in settings)
            {
                if (idx > 0)
                    sb.Append(',');
                sb.Append(v.ToString());
                idx++;
            }
            return sb.ToString();
        }

        #endregion Custom Settings

        #region HoveringCell (updated by WorkWindow)

        private string hoveringCell = "";

        public string HoveringCell
        {
            get { return hoveringCell; }
            set
            {
                hoveringCell = value;
                OnPropertyChanged("HoveringCell");
            }
        }

        #endregion HoveringCell (updated by WorkWindow)

        #region Dispose and Destructor
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // 釋放託管資源
                    if (settings != null)
                    {
                        settings.Clear();
                        settings = null;
                    }
                }

                // 釋放非託管資源
                //if (unmanagedResource != IntPtr.Zero)
                //{
                //    // 釋放資源
                //    unmanagedResource = IntPtr.Zero;
                //}

                _isDisposed = true;
            }
        }
        ~SplitCtrlVM()
        {
            Dispose(false);
            settings?.Clear();
        }
        #endregion
    }
}