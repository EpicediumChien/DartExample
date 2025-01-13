using DDPM.SA.Common.Display;
using DDPM.SA.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using System.Collections.ObjectModel;
using DDPM.SA.Common.Popup;
using System.Printing;
using System.Security.AccessControl;
using Rectangle = System.Drawing.Rectangle;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft.Win32;
using DDPM.SA.Resources.Helper;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for SaveCustomWindow.xaml
    /// </summary>
    public partial class SaveCustomWindow : Window
    {
        #region Private Members
        private readonly IDeviceManagerSA _deviceManagerSA;
        private readonly SaveCustomWindowViewModel _viewModel;
        private readonly EAArgs _eaArgs;
        private IntPtr _hWnd;
        private SplitJson[] _savedCustomList; //Will be update/reloaded at ShowAndEdit()
        private SplitJson _inputSplit; //The copy from EAArgs when entering ShowAndEdit()
        private Screen _workScreen; //Unused, use _workingArea instead
        private Rectangle _workingArea;
        #endregion Private Members

        #region Multiligual Strings
        //[ResourceKey] [Custom_layout]
        private string _customLayout = LangHelper.Instance["Custom_layout"];// "Custom layout";
        //[Save_0]
        private string _saveButton = LangHelper.Instance["Save_0"];//"Save";
        //[Cancel_0]
        private string _cancelButton = LangHelper.Instance["Cancel_0"];//"Cancel";
        //[EABroker_updateToEmProfilePrompt]
        private string _updateToEmProfilePrompt = LangHelper.Instance["EABroker_updateToEmProfilePrompt"];//"There are Easy Memory profiles associated to this custom layout.\nSaving this custom layout will update the layout for all the associated profiles.\nDo you want to save the layout?";
        //[Yes]
        private string _yesButton = LangHelper.Instance["Yes"];//"Yes";
        //[No]
        private string _noButton = LangHelper.Instance["No"];//"No";
        //[Arrange_Windows]
        private string _arrangeWindows = LangHelper.Instance["Arrange_Windows"];//"Arrange Windows";
        //[EA_MSG_0]
        private string _adjust = LangHelper.Instance["EA_MSG_0"];//"Adjust your current window arrangement, edit the name (if desired), and click \"Save\" to store the arrangement.";
        #endregion

        #region Input/Output

        //Input:
        //Setup before calling ShowAndEdit()
        public EventHandler<string>? SaveButtonClick;
        public EventHandler<string>? CancelButtonClick;
        //Robert_Lin, 2024-11-10, not need any more, will load from UserSettings.CustomList
        //public string CustomName;
        //public List<string> CustomNames;

        //Output:
        public SplitJson SelectedCustomItem
        {
            get { return _viewModel.SelectedCustomItem; }
        }

        //Unused, using WorkingArea instead
        public Screen WorkScreen { get { return _workScreen; } }
        public Rectangle WorkingArea => _workingArea;


        #endregion Input/Output

        #region ctors
        /// <summary>
        /// Create a SaveCustomWindow for Overlap layout editor.
        /// <param name="deviceManager">Used to get CustomNames by reading EACustomList from settings file.</param>
        /// <param name="eaArgs">The EA layout info to be edited.</param>
        /// <param name="eaArgs">The EA layout info to be edited.</param>
        /// </summary>
        public SaveCustomWindow(IDeviceManagerSA deviceManager, EAArgs eaArgs, int x, int y)
        {
            InitializeComponent();
            _deviceManagerSA = deviceManager;
            _eaArgs = eaArgs;
            _viewModel = new SaveCustomWindowViewModel();
            DataContext = _viewModel;

            if ((_eaArgs != null) && (_eaArgs.SplitJson != null))
                _viewModel.IsOverlapLayout = _eaArgs.SplitJson.IsOverlapLayout;

            _inputSplit = eaArgs.SplitJson;

            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            double dpiX = CommonFunctions.GetDpiX();
            //double dpiX = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    dpiX = (double)varX / (double)96;
            //}
            Left = x / dpiX;
            Top = y / dpiX;
        }
        public SaveCustomWindow(IDeviceManagerSA deviceManager)
        {
            InitializeComponent();
            _deviceManagerSA = deviceManager;
            //_viewModel = new SaveCustomWindowViewModel(_deviceManagerSA);
            _viewModel = new SaveCustomWindowViewModel();
            DataContext = _viewModel;
        }
        #endregion

        #region Init

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            _hWnd = wndHelper.Handle;
            Win32Lib.Win32.HideWinFromAltTab(_hWnd);


            //Add CustomNames to the ComboBox.Items
            //if (CustomNames != null)
            //{
            //    foreach (string name in CustomNames)
            //    {
            //        cbNames.Items.Add(name);
            //    }
            //    //Determine the selection
            //    cbNames.SelectedIndex = CustomNames.IndexOf(CustomName);
            //}
            UpdateUIContent();
            //UpdateMultilingualUiText();

            InitComboBox();

            //Determine the display position (x,y)

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                DialogResult = false;

            if (CancelButtonClick != null)
            {
                CancelButtonClick(this, "");
            }
            Hide();
        }

        //private void UpdateMultilingualUiText()
        //{
        //    //txtCustomLayout.Text = _customLayout;
        //    saveBtn.Content = _saveButton;
        //    cancelBtn.Content = _cancelButton;

        //}

        /// <summary>
        /// Update UI content based on layout type:
        /// If editing layout is Overlap,
        ///    HeaderText="{_arrangeWindows}", SubText="{_adjust}"
        /// Else (preset layout)
        ///    HeaderText="{_customLayout}", SubText=""
        /// </summary>
        private void UpdateUIContent()
        {
            if (_viewModel.IsOverlapLayout)
            {
                _viewModel.HeaderText = _arrangeWindows;
                _viewModel.SubText = _adjust;
                _viewModel.IsAdjustTextVisible = true;
            }
            else
            {
                _viewModel.HeaderText = _customLayout;
                _viewModel.SubText = _adjust;
                _viewModel.IsAdjustTextVisible = false;
            }
            saveBtn.Content = _saveButton;
            cancelBtn.Content = _cancelButton;
        }
        #endregion

        #region CustomNames
        private void InitComboBox()
        {
            if (_deviceManagerSA == null)
                return;
            if (_eaArgs == null)
                return;

            //Build ComboBox ItemsSource and determine SelectedItem
            //
            if (_savedCustomList != null)
            {
                Array.Clear(_savedCustomList);
                _savedCustomList = null;
            }

            //1 Load CustomList from UserSettings file
            int customId = 1;
            ObservableCollection<SplitJson> tempList = new ObservableCollection<SplitJson>();
            _savedCustomList = _deviceManagerSA.ReadEACustomList().Result;
            if (_savedCustomList != null && _savedCustomList.Length > 0)
            {
                //A2 Add saved custom into ComboBoxItems
                foreach (SplitJson custom in _savedCustomList)
                {
                    if (custom.CustomId == 0)
                        custom.CustomId = customId;
                    SplitJson cbItem = custom.Clone();
                    tempList.Add(cbItem);
                    customId++;
                }
            }

            //2 If the tempList.Count>=5, to determine the selectedItem
            // caseNo                   SelectedItem
            // 1_Add from Preset        The oldest of saved custom list
            // 2_Add from Overlap       The oldest of saved custom list
            // 3_Edit from PresetCustom Current item (in EAArgs, find the matched EAID)
            //
            //Where 'the oldest of saved custom list' will be the first item of the list
            //that is: savedCustomList[0] = tempList[0]
            int caseNo = 1;
            if (_eaArgs.SplitJson.EAID >= EAEMConstants.EAID_FirstCustom)
                caseNo = 3;
            if (_eaArgs.SplitJson.IsOverlapLayout)
                caseNo = 2;
            if (tempList.Count >= EAEMConstants.MaxCustomItems)
            {
                _viewModel.CustomList = tempList;
                if (caseNo == 3)
                {
                    SplitJson? selItem = tempList.FirstOrDefault(x => x.EAID == _eaArgs.SplitJson.EAID);
                    if (selItem != null)
                        _viewModel.SelectedCustomItem = selItem;
                }
                else //Case 1 and 2
                {
                    //Select null, but set a default name
                    _viewModel.SelectedCustomItem = null;
                    //cbNames.IsReadOnly = true;
                    //cbNames.Text = "Please select from drop-down list";
                }
            }
            else
            {
                //3 tempList.Count<5, 
                //3.1 Add unused default CustomNames to list
                //3.2 Determine the selected item

                //3.1 Add default custom names: ["Custom Layout (1)" ... "Custom Layout (5)"]
                //    Add the the names which is not in saved custom list, until item count == 5
                int nameNo = 1;
                bool isTheFirstDefaultName = true;
                //string selName = "Custom Layout (1))";
                string selName = LangHelper.Instance["Custom_layout"] + @" (1)";
                while (tempList.Count < EAEMConstants.MaxCustomItems)
                {
                    //generate the default custom name

                    //string customName = $"Custom Layout ({nameNo})";
                    string customName = $"{LangHelper.Instance["Custom_layout"]} ({nameNo})";
                    //Check if the name is existed
                    if (tempList.FirstOrDefault(x => x.CustomName.Equals(customName)) == null)
                    {
                        //Not exist (not in-used) => Add into list 
                        //EAID=0 in ComboBox means this SplitJson is not in-used layout
                        SplitJson splitJson = new SplitJson()
                        {
                            CustomName = customName,
                            CustomId = customId,
                            EAID = 0
                        };
                        customId++;
                        tempList.Add(splitJson);
                        if (isTheFirstDefaultName)
                        {
                            selName = customName;
                            //_viewModel.SelectedCustomItem = splitJson;
                            isTheFirstDefaultName = false;
                        }
                    }
                    nameNo++;
                }
                _viewModel.CustomList.Clear();
                _viewModel.CustomList = new ObservableCollection<SplitJson>(tempList);
                _viewModel.SelectedCustomItem = _viewModel.CustomList.FirstOrDefault(x => x.CustomName.Equals(selName));

                //3.2 Determine the selected item
                //
                // caseNo                   SelectedItem
                // 1_Add from Preset        The first item of default custom name
                // 2_Add from Overlap       The first item of default custom name
                // 3_Edit from PresetCustom Current item (in EAArgs, find the matched EAID)

                if (caseNo == 3)
                {
                    SplitJson? selItem = tempList.FirstOrDefault(x => x.EAID == _eaArgs.SplitJson.EAID);
                    if (selItem != null)
                        _viewModel.SelectedCustomItem = selItem;
                }
                else //Case 1 and 2
                {
                    //Already selected
                }
            }
        }
        #endregion

        #region ShowAndEdit
        //v1, Robert_Lin, 2024-11-19, unused, dont use and test
        public void ShowAndEdit_v1(EAArgs arg, Screen scr)
        {
            _inputSplit = arg.SplitJson;
            _workScreen = scr;

            this.Dispatcher.Invoke(() =>
            {
                Trace.WriteLine($"  * EAArgs.CustomName=[{arg.SplitJson.CustomName}]");

                //Update UI
                _viewModel.IsOverlapLayout = arg.SplitJson.IsOverlapLayout;
                if (_viewModel.IsOverlapLayout)
                {
                    _viewModel.HeaderText = _arrangeWindows;
                    _viewModel.IsAdjustTextVisible = true;
                }
                else
                {
                    _viewModel.HeaderText = _customLayout;
                    _viewModel.SubText = _adjust;
                    _viewModel.IsAdjustTextVisible = false;
                }

                //Build ComboBox ItemsSource and determine SelectedItem
                //

                //1 Load CustomList from UserSettings file
                int customId = 1;
                ObservableCollection<SplitJson> tempList = new ObservableCollection<SplitJson>();
                _savedCustomList = _deviceManagerSA.ReadEACustomList().Result;
                if (_savedCustomList != null && _savedCustomList.Length > 0)
                {
                    //A2 Add saved custom into ComboBoxItems
                    foreach (SplitJson custom in _savedCustomList)
                    {
                        if (custom.CustomId == 0)
                            custom.CustomId = customId;
                        SplitJson cbItem = custom.Clone();
                        tempList.Add(cbItem);
                        customId++;
                    }
                }

                //2 If the tempList.Count>=5, to determine the selectedItem
                // caseNo                   SelectedItem
                // 1_Add from Preset        The oldest of saved custom list
                // 2_Add from Overlap       The oldest of saved custom list
                // 3_Edit from PresetCustom Current item (in EAArgs, find the matched EAID)
                //
                //Where 'the oldest of saved custom list' will be the first item of the list
                //that is: savedCustomList[0] = tempList[0]
                int caseNo = 1;
                if (arg.SplitJson.EAID >= EAEMConstants.EAID_FirstCustom)
                    caseNo = 3;
                if (arg.SplitJson.IsOverlapLayout)
                    caseNo = 2;
                if (tempList.Count >= EAEMConstants.MaxCustomItems)
                {
                    _viewModel.CustomList = tempList;
                    if (caseNo == 3)
                    {
                        SplitJson? selItem = tempList.FirstOrDefault(x => x.EAID == arg.SplitJson.EAID);
                        if (selItem != null)
                            _viewModel.SelectedCustomItem = selItem;
                    }
                    else //Case 1 and 2
                    {
                        //Select null, but set a default name
                        _viewModel.SelectedCustomItem = null;
                        //cbNames.IsReadOnly = true;
                        //cbNames.Text = "Please select from drop-down list";
                    }
                }
                else
                {
                    //3 tempList.Count<5, 
                    //3.1 Add unused default CustomNames to list
                    //3.2 Determine the selected item

                    //3.1 Add default custom names: ["Custom Layout (1)" ... "Custom Layout (5)"]
                    //    Add the the names which is not in saved custom list, until item count == 5
                    int nameNo = 1;
                    bool isTheFirstDefaultName = true;
                    string selName = "Custom Layout (1))";
                    while (tempList.Count < EAEMConstants.MaxCustomItems)
                    {
                        //generate the default custom name
                        string customName = $"Custom Layout ({nameNo})";
                        //Check if the name is existed
                        if (tempList.FirstOrDefault(x => x.CustomName.Equals(customName)) == null)
                        {
                            //Not exist (not in-used) => Add into list 
                            //EAID=0 in ComboBox means this SplitJson is not in-used layout
                            SplitJson splitJson = new SplitJson()
                            {
                                CustomName = customName,
                                CustomId = customId,
                                EAID = 0
                            };
                            customId++;
                            tempList.Add(splitJson);
                            if (isTheFirstDefaultName)
                            {
                                selName = customName;
                                //_viewModel.SelectedCustomItem = splitJson;
                                isTheFirstDefaultName = false;
                            }
                        }
                        nameNo++;
                    }
                    _viewModel.CustomList.Clear();
                    _viewModel.CustomList = new ObservableCollection<SplitJson>(tempList);
                    _viewModel.SelectedCustomItem = _viewModel.CustomList.FirstOrDefault(x => x.CustomName.Equals(selName));

                    //3.2 Determine the selected item
                    //
                    // caseNo                   SelectedItem
                    // 1_Add from Preset        The first item of default custom name
                    // 2_Add from Overlap       The first item of default custom name
                    // 3_Edit from PresetCustom Current item (in EAArgs, find the matched EAID)

                    if (caseNo == 3)
                    {
                        SplitJson? selItem = tempList.FirstOrDefault(x => x.EAID == arg.SplitJson.EAID);
                        if (selItem != null)
                            _viewModel.SelectedCustomItem = selItem;
                    }
                    else //Case 1 and 2
                    {
                        //Already selected
                    }


                }
                //cbNames.Items.Clear();
                //string selectedName = arg.SplitJson.CustomName;
                //if ((arg.CustomNames != null) && (arg.CustomNames.Count > 0))
                //{
                //    int addCount = 0;
                //    foreach (string name in arg.CustomNames)
                //    {
                //        string addName = name;
                //        //Check length of name
                //        if (addName.Length > EAEMConstants.MaxCustomNameLenth)
                //            addName = addName.Substring(0, EAEMConstants.MaxCustomNameLenth);
                //        cbNames.Items.Add((string)addName);
                //        addCount++;
                //        if (addCount >= EAEMConstants.MaxCustomItems)
                //            break;
                //    }
                //    cbNames.SelectedItem = selectedName;
                //}
                //else //CustomNames is empty
                //{
                //    //Add one item to ComboBox
                //    if (String.IsNullOrWhiteSpace(selectedName))
                //    {
                //        selectedName = "Custom Layout (1)";
                //    }
                //    cbNames.Items.Add(selectedName);
                //}
                //cbNames.SelectedValue = selectedName;

                //Calculate the position/size of EditWindow
                //Robert_Lin, 2024-12-6, use the method in CommonFunctions
                double dpiX = CommonFunctions.GetDpiX();
                //double dpiX = 1.000;
                //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                //if (dpiXProperty != null)
                //{
                //    var varX = (int)dpiXProperty.GetValue(null, null);
                //    dpiX = (double)varX / (double)96;
                //}


                if (_viewModel.IsOverlapLayout)
                {
                    Height = 284;
                    double x = scr.WorkingArea.Left + scr.WorkingArea.Width / 2 - Width / 2;
                    double y = scr.WorkingArea.Top + scr.WorkingArea.Height / 2 - Height / 2;

                    Left = x / (double)dpiX; ;
                    Top = y / (double)dpiX; ;
                }
                else
                {
                    Height = 196;
                    Left = scr.WorkingArea.Left / (double)dpiX;
                    Top = scr.WorkingArea.Top / (double)dpiX;

                }

                Show();
                Topmost = true;
            });
        }

        //v2 Robert_Lin, 2024-11-19 for Span monitors
        public void ShowAndEdit(EAArgs arg, Rectangle workingArea)
        {
            _inputSplit = arg.SplitJson;
            //_workScreen = scr;
            _workingArea = workingArea;

            this.Dispatcher.Invoke(() =>
            {
                Trace.WriteLine($"  * EAArgs.CustomName=[{arg.SplitJson.CustomName}]");

                //Update UI
                _viewModel.IsOverlapLayout = arg.SplitJson.IsOverlapLayout;
                if (_viewModel.IsOverlapLayout)
                {
                    _viewModel.HeaderText = _arrangeWindows;
                    _viewModel.SubText = _adjust;
                    _viewModel.IsAdjustTextVisible = true;
                }
                else
                {
                    _viewModel.HeaderText = _customLayout;
                    _viewModel.SubText = "";
                    _viewModel.IsAdjustTextVisible = false;
                }

                //Build ComboBox ItemsSource and determine SelectedItem
                //

                //1 Load CustomList from UserSettings file
                int customId = 1;
                ObservableCollection<SplitJson> tempList = new ObservableCollection<SplitJson>();
                _savedCustomList = _deviceManagerSA.ReadEACustomList().Result;
                if (_savedCustomList != null && _savedCustomList.Length > 0)
                {
                    //A2 Add saved custom into ComboBoxItems
                    foreach (SplitJson custom in _savedCustomList)
                    {
                        if (custom.CustomId == 0)
                            custom.CustomId = customId;
                        SplitJson cbItem = custom.Clone();
                        tempList.Add(cbItem);
                        customId++;
                    }
                }

                //2 If the tempList.Count>=5, to determine the selectedItem
                // caseNo                   SelectedItem
                // 1_Add from Preset        The oldest of saved custom list
                // 2_Add from Overlap       The oldest of saved custom list
                // 3_Edit from PresetCustom Current item (in EAArgs, find the matched EAID)
                //
                //Where 'the oldest of saved custom list' will be the first item of the list
                //that is: savedCustomList[0] = tempList[0]
                int caseNo = 1;
                if (arg.SplitJson.EAID >= EAEMConstants.EAID_FirstCustom)
                    caseNo = 3;
                if (arg.SplitJson.IsOverlapLayout)
                    caseNo = 2;
                if (tempList.Count >= EAEMConstants.MaxCustomItems)
                {
                    _viewModel.CustomList = tempList;
                    if (caseNo == 3)
                    {
                        SplitJson? selItem = tempList.FirstOrDefault(x => x.EAID == arg.SplitJson.EAID);
                        if (selItem != null)
                            _viewModel.SelectedCustomItem = selItem;
                    }
                    else //Case 1 and 2
                    {
                        //Select null, but set a default name
                        _viewModel.SelectedCustomItem = null;
                        //cbNames.IsReadOnly = true;
                        //cbNames.Text = "Please select from drop-down list";
                    }
                }
                else
                {
                    //3 tempList.Count<5, 
                    //3.1 Add unused default CustomNames to list
                    //3.2 Determine the selected item

                    //3.1 Add default custom names: ["Custom Layout (1)" ... "Custom Layout (5)"]
                    //    Add the the names which is not in saved custom list, until item count == 5
                    int nameNo = 1;
                    bool isTheFirstDefaultName = true;
                    string selName = "Custom Layout (1))";
                    while (tempList.Count < EAEMConstants.MaxCustomItems)
                    {
                        //generate the default custom name
                        string customName = $"Custom Layout ({nameNo})";
                        //Check if the name is existed
                        if (tempList.FirstOrDefault(x => x.CustomName.Equals(customName)) == null)
                        {
                            //Not exist (not in-used) => Add into list 
                            //EAID=0 in ComboBox means this SplitJson is not in-used layout
                            SplitJson splitJson = new SplitJson()
                            {
                                CustomName = customName,
                                CustomId = customId,
                                EAID = 0
                            };
                            customId++;
                            tempList.Add(splitJson);
                            if (isTheFirstDefaultName)
                            {
                                selName = customName;
                                //_viewModel.SelectedCustomItem = splitJson;
                                isTheFirstDefaultName = false;
                            }
                        }
                        nameNo++;
                    }
                    _viewModel.CustomList.Clear();
                    _viewModel.CustomList = new ObservableCollection<SplitJson>(tempList);
                    _viewModel.SelectedCustomItem = _viewModel.CustomList.FirstOrDefault(x => x.CustomName.Equals(selName));

                    //3.2 Determine the selected item
                    //
                    // caseNo                   SelectedItem
                    // 1_Add from Preset        The first item of default custom name
                    // 2_Add from Overlap       The first item of default custom name
                    // 3_Edit from PresetCustom Current item (in EAArgs, find the matched EAID)

                    if (caseNo == 3)
                    {
                        SplitJson? selItem = tempList.FirstOrDefault(x => x.EAID == arg.SplitJson.EAID);
                        if (selItem != null)
                            _viewModel.SelectedCustomItem = selItem;
                    }
                    else //Case 1 and 2
                    {
                        //Already selected
                    }


                }
                //cbNames.Items.Clear();
                //string selectedName = arg.SplitJson.CustomName;
                //if ((arg.CustomNames != null) && (arg.CustomNames.Count > 0))
                //{
                //    int addCount = 0;
                //    foreach (string name in arg.CustomNames)
                //    {
                //        string addName = name;
                //        //Check length of name
                //        if (addName.Length > EAEMConstants.MaxCustomNameLenth)
                //            addName = addName.Substring(0, EAEMConstants.MaxCustomNameLenth);
                //        cbNames.Items.Add((string)addName);
                //        addCount++;
                //        if (addCount >= EAEMConstants.MaxCustomItems)
                //            break;
                //    }
                //    cbNames.SelectedItem = selectedName;
                //}
                //else //CustomNames is empty
                //{
                //    //Add one item to ComboBox
                //    if (String.IsNullOrWhiteSpace(selectedName))
                //    {
                //        selectedName = "Custom Layout (1)";
                //    }
                //    cbNames.Items.Add(selectedName);
                //}
                //cbNames.SelectedValue = selectedName;

                //Calculate the position/size of EditWindow
                //Robert_Lin, 2024-12-6, use the method in CommonFunctions
                double dpiX = CommonFunctions.GetDpiX();
                //double dpiX = 1.000;
                //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                //if (dpiXProperty != null)
                //{
                //    var varX = (int)dpiXProperty.GetValue(null, null);
                //    dpiX = (double)varX / (double)96;
                //}


                if (_viewModel.IsOverlapLayout)
                {
                    Height = 284;
                    double x = workingArea.Left + workingArea.Width / 2 - Width / 2;
                    double y = workingArea.Top + workingArea.Height / 2 - Height / 2;

                    Left = x / (double)dpiX; ;
                    Top = y / (double)dpiX; ;
                }
                else
                {
                    Height = 196;
                    Left = workingArea.Left / (double)dpiX;
                    Top = workingArea.Top / (double)dpiX;

                }

                Show();
                Topmost = true;
            });
        }
        #endregion

        #region UI Event handlers
        //Move window
        private void rootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ClickCount == 1))
            {
                this.DragMove();
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            //Required: SelectedCustomItem cannot be null
            if (SelectedCustomItem == null)
                return;

            int selEaId = SelectedCustomItem.EAID;

            //Check if the EasyMemory Profile may be conflict
            //1 If the SelectedCustomItem is associated with EasyMemory profile
            //  Only the Custom layout need to be checked
            if ((_deviceManagerSA != null) && (_inputSplit.EAID >= EAEMConstants.EAID_FirstCustom))
            {
                bool isEaIdUsedByEmProfile = _deviceManagerSA.CheckEAIDExit(new VcpCore.Common.MonitorInfo(), _inputSplit.EAID).Result;
                if (isEaIdUsedByEmProfile)
                {
                    //Pop up a meesgaeBox to confirm 
                    PopupBase msgBox = new PopupBase("", _updateToEmProfilePrompt, _noButton, _yesButton, null, false, 0);
                    msgBox.Owner = this;
                    if (msgBox.ShowDialog() != true)
                    {
                        if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                            DialogResult = false;

                        if (CancelButtonClick != null)
                        {
                            CancelButtonClick(this, "");
                        }
                        Hide();
                        return;
                    }
                }
            }

            //DDPMW-861 Item 2, If the input CustomName is duplicated with SavedCustomList
            // Rename the CustomName to {CustomName}_{No} where No is 1,2,3...
            string selCustomName = FixCustomNameFor_DDPMW861();
            if (!String.IsNullOrEmpty(selCustomName))
            {
                SelectedCustomItem.CustomName = selCustomName;
            }

            //If SaveCustomWindow is shown by ShowDialog, then we need to set DialogResult
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                DialogResult = true;

            ////If it's empty
            //if (String.IsNullOrWhiteSpace(cbNames.Text))
            //    return;
            ////Trunk the string if it too long
            //string retName = cbNames.Text;
            //if (retName.Length > EAEMConstants.MaxCustomNameLenth)
            //    retName = retName.Substring(0, EAEMConstants.MaxCustomNameLenth);
            //CustomName = retName;

            if (SaveButtonClick != null)
            {
                SaveButtonClick(this, selCustomName);
            }
            Hide();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            //If SaveCustomWindow is shown by ShowDialog, then we need to set DialogResult
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                DialogResult = false;
            if (CancelButtonClick != null)
            {
                CancelButtonClick(this, "");
            }
            Hide();
        }
        //The close X button click
        //private void closeGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (CancelButtonClick != null)
        //    {
        //        CancelButtonClick(this, "");
        //    }
        //    Hide();
        //}
        private void cbNames_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.RefreshIsSaveButtonEnabled();

            //string custName = cbNames.Text;
            //if (String.IsNullOrWhiteSpace(custName))
            //{
            //    saveBtn.IsEnabled = false;
            //}
            //else
            //{
            //    saveBtn.IsEnabled = true;
            //}
        }
        //private void closeGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (CancelButtonClick != null)
        //    {
        //        CancelButtonClick(this, "");
        //    }
        //    Hide();
        //}
        private void closeX_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //If SaveCustomWindow is shown by ShowDialog, then we need to set DialogResult
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                DialogResult = false;

            if (CancelButtonClick != null)
            {
                CancelButtonClick(this, "");
            }
            Hide();
        }
        #endregion UI Event handlers

        #region TextBox Input Validation
        private void cbNames_Loaded(object sender, RoutedEventArgs e)
        {
            //Reference: https://stackoverflow.com/questions/1572887/how-to-set-maxlength-for-combobox-in-wpf
            System.Windows.Controls.ComboBox cb = (System.Windows.Controls.ComboBox)sender;
            if (cb != null)
            {
                var partEditableTextBox = (System.Windows.Controls.TextBox)cb.Template.FindName("PART_EditableTextBox", cb);
                if (partEditableTextBox != null)
                {
                    partEditableTextBox.MaxLength = EAEMConstants.MaxCustomNameLenth;
                }
            }
        }
        #endregion

        #region CustomName
        private string FixCustomNameFor_DDPMW861()
        {
            //_inputSplit must has stored
            if (_inputSplit == null)
                return String.Empty;

            //SelectedCustomItem must have data
            if (SelectedCustomItem == null)
                return String.Empty;


            //The default return CustomName will be the user edited
            //But if SelectedCustomItem is nu
            string selCustomName = SelectedCustomItem.CustomName;
            long selCustomId = SelectedCustomItem.CustomId;

            //DDPMW-861 Item 2 "Given"
            // User rename the predefined custom layout name
            //=> Currently, we will not allow duplicate name for all cases
            //if ((!_inputSplit.IsOverlapLayout) && //Edit a predefine layout
            //    (_inputSplit.EAID < EAEMConstants.EAID_FirstCustom)) //It's not added from Preset list
            {
                //In case of user edit a preset custom layout
                //

                //Check if duplicated with saved custom list
                SplitJson? spDup = Array.Find(_savedCustomList, x => x.CustomName == selCustomName);
                //If duplicate name found
                if (spDup != null)
                {
                    //But it's the inputSplit itself, user did not change CustomName
                    if (spDup.CustomId == selCustomId)
                    //if (spDup.EAID == _inputSplit.EAID)
                    {
                        //Used the user selected name
                        return selCustomName;
                    }
                    else
                    {
                        //Not original input item and dupliacte with other item, need to rename
                        for (int i = 0; i < EAEMConstants.MaxCustomNameLenth; i++)
                        {
                            string newName = selCustomName + $"_{i + 1}";
                            spDup = Array.Find(_savedCustomList, x => x.CustomName == newName);
                            if (spDup == null)
                            {
                                return newName;
                            }
                        } //for (i)
                    } //if (spDup.EAID == _inputSplit.EAID) else
                } //if (spDup != null)
            }
            return selCustomName;
        }


        #endregion

        #region Exit
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }
        #endregion
    }
}
