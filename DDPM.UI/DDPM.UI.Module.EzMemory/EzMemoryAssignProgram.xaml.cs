using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Windows.ApplicationModel;
using DragEventArgs = System.Windows.DragEventArgs;
using ProgressBar = System.Windows.Controls.ProgressBar;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryAssignProgram.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryAssignProgram : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        private HomeDevice _selecthomeDevice;
        private double titleText_DefaultFontSize = 20;
        private double mainText_DefaultFontSize = 60;
        private double subText_DefaultFontSize = 15;
        #endregion Private Members
        public EzMemoryAssignProgram(DisplayViewModel vmDisplay, EzArrangeViewModel vm, HomeDevice _homeDeviceSelect)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _selecthomeDevice = _homeDeviceSelect;
            _log = vmDisplay.Console.CreateLog("EzMemoryAssignProgram");
            _log.Info($"{nameof(EzMemoryAssignProgram)} - Constructed");
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            Requires.NotNull(vmDisplay, nameof(vmDisplay));
            InitializeComponent();

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = vm;
            DataContext = vm;

            //Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            //_vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            //InitializePage();

            // 這裡排編號
            //_vm.ispCtrlForEm = ISplitCtrl.Create(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey);
            //Robert_Lin, 2024-11-19, ISplitCtrl.Create(EAID) can only create preset layout (EAID=[1~48]
            //_vm.ispCtrlForEm = ISplitCtrl.Create(_vm.CurrentSelectsEAID);
            //You can use Clone() to clone a ISplitCtrl from SplitIte.ISplitCtrl

            //Robert_Lin 2025-1-9 change to CurrentEditSelectspItem
            //NEW:
            _vm.ispCtrlForEm = _vm.CurrentEditSelectspItem.ISplitCtrl.Clone();
            //OLD:
            //_vm.ispCtrlForEm = _vm.SelectedSplitItem.ISplitCtrl.Clone();

            //_vm.ispCtrlForEm = ISplitCtrl.Create(_vm.SelectedSplitItem.);
            _vm.ispCtrlForEm!.IsEditable = false; //If you do need the 'pencil' icon, please set it to false
            _vm.ispCtrlForEm.SplitMode = eSplitModes.Em;
            EMsplitCtrl.Content = _vm.ispCtrlForEm.UC;

            //int _no = 1;
            //foreach (var cellBorder in _vm.ispCtrlForEm.CellList)
            //{
            //    cellBorder.CellBd.CellNumber = _no;
            //    cellBorder.CellBd.MemoryText = _no.ToString();
            //    //cellBorder.CellBd.MemoryImage = _vm.ImageSource;
            //    _vm.AlignCellNumberAndAppName(_no, cellBorder);
            //    _vm?.RegisterCellBorder(cellBorder.CellBd, _no);
            //    _no++;
            //}
            //Record UXTextBox
            if (_vm.ispCtrlForEm.CellList.Count <= 2)
            {
                _vm.currentUXTextBoxInfo = Window1_1TextBlock;
            }
            else
            {
                _vm.currentUXTextBoxInfo = Window1TextBlock;
            }

            UserControl_Loaded(null, null);
            InitializePage();


            int _no = 1;
            foreach (var cellBorder in _vm.ispCtrlForEm.CellList)
            {
                cellBorder.CellBd.CellNumber = _no;
                cellBorder.CellBd.MemoryText = _no.ToString();
                //cellBorder.CellBd.MemoryImage = _vm.ImageSource;
                _vm.AlignCellNumberAndAppName(_no, cellBorder);
                _vm?.RegisterCellBorder(cellBorder.CellBd, _no);
                _no++;
            }
            titleText_DefaultFontSize = TitleText.FontSize;
            mainText_DefaultFontSize = MainText.FontSize;
            subText_DefaultFontSize = SubText.FontSize;
            LeftGrid.SizeChanged -= AdjustFontSizeForWWO;
            LeftGrid.SizeChanged += AdjustFontSizeForWWO;
            Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Application.Current.MainWindow.ActualHeight > 765)
            {
                MainPanel.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                MainPanel.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        ~EzMemoryAssignProgram()
        {
            LeftGrid.SizeChanged -= AdjustFontSizeForWWO;
            Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
        }

        /// <summary>
        /// Initialize Page, get Split window count, set string
        /// </summary>
        public void InitializePage()
        {
            //這裡加入分割視窗的個數
            //Robert_Lin 2025-1-10 During editing, the SplitItem should be CurrentEditSelectspItem
            //NEW:
            int splitCount = 0;
            if (_vm.CurrentEditSelectspItem != null)
            {
                if (_vm.CurrentEditSelectspItem.ISplitCtrl != null)
                {
                    splitCount = _vm.CurrentEditSelectspItem.ISplitCtrl.CellList.Count;
                }
            }
            if (splitCount == 2)
            //OLD:
            //if (_vm.SelectedSplitItem.ISplitCtrl.CellList.Count == 2)//(_vm.SelectedSplitItem.CellCount == 2)
            {
                _vm.IsRightGridPage2Visible = true;
                _vm.SelectedValue = 2;
            }
            else
            {
                _vm.IsRightGridPage2Visible = false;
                //Robert_Lin 2025-1-10, editing SplitItem should be CurrentEditSelectspItem
                //NEW:
                if (_vm.CurrentEditSelectspItem != null)
                {
                    _vm.SelectedValue = splitCount;
                }
                //OLD:
                //_vm.SelectedValue = _vm.SelectedSplitItem.ISplitCtrl.CellList.Count;// _vm.SelectedSplitItem.CellCount;
            }
            _vm.ezPages = _vm.GetEzPages();

            if (_vm.ezPages.ContainsKey(_vm._currentDeviceModel))
            {
                _vm.CurrentAnimationPage = _vm.ezPages[_vm._currentDeviceModel].Count;
                var pageData = _vm.ezPages[_vm._currentDeviceModel][1];
                MainText.Text = pageData.MainText!;
                SubText.Text = pageData.SubText!;
            }

            SyncEditStatusForAssignPage(false);
            ////編輯模式但不是由AddPage返回才執行
            //if (_vm.IsEditProfile && !_vm.IsAddPageBack)
            //{
            //    SyncEditStatusForAssignPage(false);
            //}
            //else
            //{
            //    SyncEditStatusForAssignPage(false);
            //}
        }

        /// <summary>
        /// Sync Edit Status 回填App Name
        /// </summary>
        public void SyncEditStatusForAssignPage(bool shouldClearApps=true)
        {
            if (shouldClearApps)
            {
                _vm._sortApps.Clear();
            }
            //if (_vm.currentEditprofile == null)
            {
                if (_vm._sortApps.Count > 0)
                {
                     _vm.RefreshSortAppsKeysForNewCellCount(_vm.SelectedValue);

                    int splitCount = Math.Min(_vm.SelectedValue, _vm._sortApps.Count);

                    for (int i = 0; i < splitCount; i++)
                    {
                        string buttonName = "AddButton" + (i + 1).ToString();

                        if (_vm.SelectedValue <= 2)
                        {
                            buttonName = "AddButton2_" + (i + 1).ToString();
                        }

                        if (_vm._sortApps.ContainsKey(buttonName))
                        {
                            _vm.UpdateTextBlockAppName(buttonName, _vm._sortApps[buttonName].AppName);
                        }
                    }
                    return;
                }
                if (_vm.currentEditprofile == null)
                    return;
            }

            int loopCount = Math.Min(_vm.SelectedValue, _vm.currentEditprofile.AppInfos.Count);

            for (int i = 0; i < loopCount; i++)
            {
                Bind_AddFullPage_AppCollectionData newApp;// = new Bind_AddFullPage_AppCollectionData
                var appInfo = _vm.currentEditprofile.AppInfos[i];
                // 檢查 _totalApps 中是否有相同的 AppName
                var existingApp = _vm._bind_apps.FirstOrDefault(app => app.AppPath.ToUpper() == appInfo.Path.ToUpper());
                if (existingApp != null)
                {
                    //appData.AppIcon = existingApp.AppIcon;
                    newApp = new Bind_AddFullPage_AppCollectionData
                    {
                        AppName = existingApp.AppName,
                        AppPath = existingApp.AppPath,
                        AppUserModelID = existingApp.AppUserModelID,
                        AppType = existingApp.AppType,// ? "True" : "False",
                        InstalledDate = DateTime.Now,
                        AppIcon = existingApp.AppIcon
                    };
                }
                else
                {
                    //appData.AppIcon = fileName[index].Image.ToString();
                    newApp = new Bind_AddFullPage_AppCollectionData
                    {
                        AppName = appInfo.Name,
                        AppPath = appInfo.Path,
                        AppUserModelID = appInfo.AppUserModelID,
                        AppType = appInfo.IsUWP ? "True" : "False",
                        InstalledDate = DateTime.Now,
                        AppIcon = "Assets/palette.png"
                    };
                }

                string buttonName = "AddButton" + (i + 1).ToString();

                if (_vm.SelectedValue <= 2)
                {
                    buttonName = "AddButton2_" + (i + 1).ToString();
                }

                _vm.UpdateTextBlockAppName(buttonName, appInfo.Name);
                if (_vm._sortApps.ContainsKey(buttonName))
                {
                    _vm._sortApps[buttonName] = newApp;
                }
                else
                {
                    _vm._sortApps.Add(buttonName, newApp);
                }
            }

            _vm.RefreshAssignPageButtons();
        }

        /// <summary>
        /// Next Page
        /// </summary>
        public void NextPage()
        {
            //前進 AddPage 前設False
            _vm.IsAddPageBack = false;
            _vm._currentPageIndex++;
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
            //UpdatePageContent();
        }

        /// <summary>
        /// Previous Page
        /// </summary>
        public void PreviousPage()
        {
            if (_vm._currentPageIndex > 0)
            {
                _vm._currentPageIndex--;
                UpdatePageContent();
            }
        }

        /// <summary>
        /// Update Page Content
        /// </summary>
        public void UpdatePageContent()
        {
            if (_vm._currentPageIndex >= 3)
            {
                CancelBtn_Click(null!, null!);
            }
            var pageData = _vm.ezPages["EzMemory"][_vm._currentPageIndex];
            MainText.Text = pageData.MainText!;
            SubText.Text = pageData.SubText!;
        }

        /// <summary>
        /// Back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            //Robert_Lin 2025-1-10 When Back button clicked, we should keep _sortApp
            //When we come back from First view, we will reused _sortApps
            //So comment-out the following code
           // _vm.ClearTextBlockAppName();

            //_vm.IsEditProfile = true;// 從Aassign退回First
            EzMemoryFirst ezMemoryFirst = new EzMemoryFirst(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezMemoryFirst);
        }

        /// <summary>
        /// Next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_vm._sortApps.Count < _vm.SelectedValue)
                return;
            _vm._currentPageIndex++;
            EzMemoryLaunchOption _ezMemoryLaunchOption = new EzMemoryLaunchOption(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryLaunchOption);
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.ProgressValue = 1;
            _vm.IsAddPageBack = false;
            _vm.RightViewDataClear();
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
            return;
        }

        /// <summary>
        /// Do Progress Animation
        /// </summary>
        /// <param name="isForward"></param>
        private void DoProgressAnimation(bool isForward)
        {
            double newProgressValue;
            if (isForward)
            {
                // Move
                newProgressValue = Math.Min(_vm.ProgressValue + 1, _vm.CurrentAnimationPage);
            }
            else
            {
                // Back
                newProgressValue = Math.Max(_vm.ProgressValue - 1, 1);
            }

            DoubleAnimation progressAnimation = new DoubleAnimation
            {
                From = _vm.ProgressValue,
                To = newProgressValue,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)), // Time
                FillBehavior = FillBehavior.HoldEnd
            };

            EzMemoryProgressbar.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);

            // refresh ProgressValue
            _vm.ProgressValue = newProgressValue;
        }

        /// <summary>
        /// Add application Button1 Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _log.Info($"[EzMemoryAssignProgram] AddButton1_Click ... in");

                if (sender is System.Windows.Controls.Button button)
                {
                    string buttonName = button.Name;
                    var image = button.Template.FindName("PART_Image", button) as Image;

                    if (image != null)
                    {
                        string source = image.Source.ToString();
                        string imageState = source.Contains("EzAdd.png") ? "EzAdd" : "EzRemove";

                        if (imageState == "EzRemove")
                        {
                            if (buttonName == "AddButton2_1" || buttonName == "AddButton1")
                            {
                                _vm.UpdateTextBlockAppName("AddButton2_1", "");
                                _vm.UpdateTextBlockAppName("AddButton1", "");
                                _vm._sortApps.Remove("AddButton2_1");
                                _vm._sortApps.Remove("AddButton1");
                            }
                            else if (buttonName == "AddButton2_2" || buttonName == "AddButton2")
                            {
                                _vm.UpdateTextBlockAppName("AddButton2_2", "");
                                _vm.UpdateTextBlockAppName("AddButton2", "");
                                _vm._sortApps.Remove("AddButton2_2");
                                _vm._sortApps.Remove("AddButton2");
                            }
                            else
                            {
                                _vm.UpdateTextBlockAppName(buttonName, "");
                                _vm._sortApps.Remove(buttonName);
                            }

                            int cellno = _vm.GetTextBlockNumber(buttonName);

                            int _no = 1;
                            foreach (var cellBorder in _vm.ispCtrlForEm.CellList)
                            {
                                if (_no == cellno)
                                {
                                    cellBorder.CellBd.CellNumber = _no;
                                    cellBorder.CellBd.MemoryText = _no.ToString();
                                    cellBorder.CellBd.MemoryImage = null;
                                    //_vm.AlignCellNumberAndAppName(_no, cellBorder);
                                }
                                _no++;
                            }
                            _vm.RefreshAssignPageButtons();
                            return;
                        }
                        else
                        {
                            _vm.ButtonName = button.Name;
                            EzMemoryAddApplication _ezMemoryAddApplication = new EzMemoryAddApplication(_vmDisplay, _vm, _selecthomeDevice);
                            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAddApplication);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[EzMemoryAssignProgram] AddButton1_Click Exception occurred: {ex.Message}");
            }
            _vm.RefreshAssignPageButtons();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _log.Info($"@{nameof(EzMemoryAssignProgram)} UserControl_Loaded: ... in");

                if (_vm._bind_apps.Count != 0 && _vm._apps_all.Count != 0)
                    return;
                _vm._bind_apps.Clear();
                _vm._apps_all.Clear();

                Dictionary<string, InstalledAppInfo> data = DdpmCommonHelper.DeviceManagerSA.GetAllAppList().Result;

                if (data == null)
                {
                    _log.Info($"@{nameof(EzMemoryAssignProgram)} GetAllAppList NULL ... in");
                    return;
                }

                string strFolder = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
                strFolder += "\\";

                if (!System.IO.Directory.Exists(strFolder))
                    System.IO.Directory.CreateDirectory(strFolder);

                foreach (KeyValuePair<string, InstalledAppInfo> kvp in data)
                {
                    //if (kvp.Value.AppInstallPath.ToLower().Contains("program files"))
                    //{
                        Bind_AddFullPage_AppCollectionData new_Appdata = new Bind_AddFullPage_AppCollectionData();

                        new_Appdata.AppName = kvp.Value.AppName;
                        new_Appdata.InstalledDate = kvp.Value.lastModifyTime;
                        new_Appdata.AppPath = kvp.Value.AppInstallPath;
                        new_Appdata.AppUserModelID = kvp.Value.AppUserModelID;
                        new_Appdata.AppType = kvp.Value.isDesktopApp.ToString();

                        if (System.IO.File.Exists(strFolder + kvp.Value.IconName + ".png"))
                        {
                            new_Appdata.AppIcon = strFolder + kvp.Value.IconName + ".png";
                        }
                        else
                        {
                            new_Appdata.AppIcon = "Assets/palette.png";
                        }

                        _vm._bind_apps.Add(new_Appdata);
                        _vm._apps_all.Add(new_Appdata);
                    //}

                }
            }
            catch (Exception ex)
            {
                _log.Error($"@{nameof(EzMemoryAssignProgram)} UserControl_Loaded: Error occurred - {ex.Message}");
            }
        }

        private void Window_TextBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string controlName = _vm.UXTextBoxNameToUXButtonName(textBox.Name);
                if (_vm._sortApps.ContainsKey(controlName))
                {
                    var toolTipContent = new TextBlock();
                    if (_vm._sortApps[controlName].AppUserModelID == "")
                    {
                        toolTipContent = new TextBlock
                        {
                            Text = _vm._sortApps[controlName].AppPath,
                        };
                    }
                    else
                    {
                        toolTipContent = new TextBlock
                        {
                            Text = _vm._sortApps[controlName].AppName,
                        };
                    }

                    toolTipContent.Style = (Style)FindResource("ToolTipTextBlockStyle");

                    var toolTip = new ToolTip
                    {
                        Content = toolTipContent,
                    };

                    textBox.ToolTip = toolTip;
                }
                else
                {
                    textBox.ToolTip = string.Empty;
                }
            }
            return;
        }


        #region Adjust Text for RWD
        private void AdjustFontSizeForWWO(object sender, RoutedEventArgs e)
        {
            if (TitleText != null && MainText != null && SubText != null)
            {
                // Smaller
                Typeface typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                double txtRowMinWidth = GetLongestWordPixelLength(TitleText.Text, typeface, TitleText.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    TitleText.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(TitleText.Text, typeface, TitleText.FontSize);
                }
                else if (TitleText.FontSize < titleText_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(TitleText.Text, typeface, TitleText.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        TitleText.FontSize += 2;
                    }
                }

                txtRowMinWidth = GetLongestWordPixelLength(MainText.Text, typeface, MainText.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    MainText.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(MainText.Text, typeface, MainText.FontSize);
                }
                else if (MainText.FontSize < mainText_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(MainText.Text, typeface, MainText.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        MainText.FontSize += 2;
                    }
                }

                txtRowMinWidth = GetLongestWordPixelLength(SubText.Text, typeface, SubText.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    SubText.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(SubText.Text, typeface, SubText.FontSize);
                }
                else if (SubText.FontSize < subText_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(SubText.Text, typeface, SubText.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        SubText.FontSize += 2;
                    }
                }
            }
        }

        private double GetLongestWordPixelLength(string text, Typeface typeface, double fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Split the text into words
            string[] words = text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            double maxPixelWidth = 0;

            foreach (string word in words)
            {
                double wordWidth = 0;

                if (typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                {
                    foreach (char c in word)
                    {
                        if (glyphTypeface.CharacterToGlyphMap.TryGetValue(c, out ushort glyphIndex))
                        {
                            // Calculate width based on advance widths
                            double advanceWidth = glyphTypeface.AdvanceWidths[glyphIndex];
                            wordWidth += advanceWidth * fontSize;
                        }
                    }
                }

                maxPixelWidth = Math.Max(maxPixelWidth, wordWidth);
            }

            return maxPixelWidth;
        }
        #endregion
    }

    /// <summary>
    /// Binding change value
    /// </summary>
    public class WindowGridVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedValue && parameter is string gridIndexString && int.TryParse(gridIndexString, out int gridIndex))
            {
                // 如果 selectedValue 大於等於 gridIndex，則顯示 (Visible)，否則隱藏 (Collapsed)
                return selectedValue >= gridIndex ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
