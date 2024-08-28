using DDPM.Easy.Common;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System.Reflection;
using System.Windows;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    /// <summary>
    /// Interaction logic for EAEditWindow.xaml
    /// </summary>
    public partial class EAEditWindow : Window
    {
        private ILog? _log;

        //private SaveCustomWindow saveCustomWindow;
        private string _orgFriendlyName = string.Empty;

        private string _lastError = string.Empty;

        public EAEditWindow()
        {
            InitializeComponent();
        }

        //private void OpenSaveCustomWindow()
        //{
        //    SaveCustomWindow saveCustomWindow = new SaveCustomWindow();
        //    saveCustomWindow = new SaveCustomWindow();
        //    saveCustomWindow.Owner = this;
        //    saveCustomWindow.Left = this.Left;
        //    saveCustomWindow.Top = this.Top;
        //    saveCustomWindow.CustomName = _orgFriendlyName;
        //    saveCustomWindow.CustomNames = _inputArgs.CustomNames;
        //    saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
        //    saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
        //    saveCustomWindow.Show();
        //}
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _log = EAPlugin.PluginIoc?.GetService<ILog>();
            _log?.Info($"EAEditWindow_Loaded, Input: {inputSplitCtrl.CellCount}{inputSplitCtrl.SplitKey}, [{inputSplitCtrl.SettingsString}]");

            //saveCustomWindow = new SaveCustomWindow();
            //saveCustomWindow.Owner = this;
            //saveCustomWindow.Left = this.Left;
            //saveCustomWindow.Top = this.Top;
            //saveCustomWindow.CustomName = _orgFriendlyName;
            //saveCustomWindow.CustomNames = _inputArgs.CustomNames;
            //saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
            //saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
            //saveCustomWindow.Show();

            //InitDragDlg();
        }

        #region Call out events

        public event EventHandler EditStarted;

        public event EventHandler<string> EditCompleted;

        public event EventHandler<EAArgs> EditReturn;

        #endregion Call out events

        #region Input SplitCtrl

        private ISplitCtrl inputSplitCtrl = new SplitCtrl4A();
        private EAArgs _inputArgs;

        public bool SetSplitCtrl(int cellCount, char splitKey, List<double> settings, bool isVertical = false)
        {
            ISplitCtrl? isplitCtrl = ISplitCtrl.Create(cellCount, splitKey);
            if (isplitCtrl == null)
            {
                _log?.Error($"EAPlugin.EAEditWindow.SetSplitCtrl(), Invalid argument: {inputSplitCtrl.CellCount}{inputSplitCtrl.SplitKey}, [{inputSplitCtrl.SettingsString}]");
                return false;
            }

            inputSplitCtrl = isplitCtrl;
            //inputSplitCtrl.Vertical = isVertical;
            inputSplitCtrl.IsEditable = true;
            inputSplitCtrl.SplitMode = eSplitModes.Edit;

            if (settings != null)
            {
                inputSplitCtrl.Settings = settings;
            }

            splitCtrl.Content = inputSplitCtrl.UC;
            return true;
        }

        /// <summary>
        /// Assign the edit arguments.
        /// Caller (EAPlugin) must call this method to assign the args before calling EAEditWindow.Show()
        /// </summary>
        /// <param name="args">The EAArgs object to specify the argument for the editing.</param>
        /// <returns>
        /// true: if the input args is accepted, and the caller (EAPlugin) can signal EditStarted to
        ///       its caller (DDPM.UI). the edit window to be display soon.        ///
        /// false: otherwise. The args is invalid, caller (EAPlugin can get the error message from EAEditWindow.LastError
        /// </returns>
        public bool SetInputArg(EAArgs args, Screen scr)
        {
            _inputArgs = args;

            this.Dispatcher.Invoke(() =>
            {
                //Try to create a ISplitCtrl to verify (cellCount,SplitKey) is valid
                ISplitCtrl? ispCtrl = ISplitCtrl.Create(args.CellCount, args.SplitKey);
                if (ispCtrl == null)
                {
                    //Invalidd CellCount+SplitKey, make the error messgae
                    _lastError = $"EAPlugin.EAEditWindow.SetInputArg(), Invalid argument: {args.CellCount}{args.SplitKey}, [{SplitCtrlVM.Double_To_String(args.Settings)}]";
                    _log?.Error(_lastError);

                    return;// false;
                }
                inputSplitCtrl = ispCtrl;
                inputSplitCtrl.IsEditable = true;
                inputSplitCtrl.SplitMode = eSplitModes.Edit;

                if (args.Settings != null)
                {
                    inputSplitCtrl.Settings = args.Settings;
                }

                _orgFriendlyName = args.CustomName;

                //Calculate the position/size of EditWindow
                double dpiX = 1.000;
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                if (dpiXProperty != null)
                {
                    var varX = (int)dpiXProperty.GetValue(null, null);
                    dpiX = (double)varX / (double)96;
                }

                splitCtrl.Content = inputSplitCtrl.UC;
                //SplitContent = inputSplitCtrl.UC;

                Left = scr.WorkingArea.Left / (double)dpiX;
                Top = scr.WorkingArea.Top / (double)dpiX;
                Width = scr.WorkingArea.Width / (double)dpiX;
                Height = scr.WorkingArea.Height / (double)dpiX;

                //OpenSaveCustomWindow();

                Show();
            });
            return true;
        }

        public string LastError
        { get { return _lastError; } }

        public List<double> GetSettings()
        {
            return inputSplitCtrl.Settings;
        }

        #endregion Input SplitCtrl

        #region SaveDlg Button Clicks

        //Unused, move to EAPlugin
        private void saveCustomWidow_CancelButtonClick(object sender, EventArgs e)
        {
            //if (EditCompleted != null)
            //    EditCompleted(this, "");

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_inputArgs);
                retArgs.Result = false;
                retArgs.Command = "EditReturn";
                retArgs.Message = "User cancel the editing.";
                EditReturn(this, retArgs);
            }
        }

        //Unused, move to EAPlugin
        private void saveCustomWidow_SaveButtonClick(object sender, EventArgs e)
        {
            //if (EditCompleted != null)
            //    EditCompleted(this, "");

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_inputArgs);
                retArgs.Result = true;
                retArgs.Settings = inputSplitCtrl.Settings;
                //retArgs.CustomName = saveCustomWindow.CustomName;
                retArgs.Command = "EditReturn";
                EditReturn(this, retArgs);
            }
        }

        #endregion SaveDlg Button Clicks

        public void InvokeClose()
        {
            this.Dispatcher.Invoke(() =>
            {
                Hide();
            });
        }
    }
}