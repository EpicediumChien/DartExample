//using System.Drawing;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.ButtonSettings
{
    /// <summary>
    /// Interaction logic for ButtonSettingsRightView.xaml
    /// </summary>
    public partial class ButtonSettingsRightView : UserControl
    {
        private readonly MouseViewModel _vm;

        private readonly SolidColorBrush buttonFocusedBKColor1 = new(System.Windows.Media.Color.FromArgb(0x99, 0x13, 0x2F, 0x54));
        private readonly SolidColorBrush buttonFocusedBKColor2 = new(System.Windows.Media.Color.FromArgb(0x99, 0x20, 0x4A, 0x82));
        private readonly Dictionary<string, string> ButtonCaptions = new();

        int SelectedActionID = -1;
        private string ActiveActionSection = "";

        public ButtonSettingsRightView(MouseViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            ButtonCaptions.Add(MouseButtonName.ScrollWheelClick.ToString(), Strings.ScrollWheelCaption);
            ButtonCaptions.Add(MouseButtonName.ScrollTiltLeft.ToString(), Strings.ScrollTiltLCaption);
            ButtonCaptions.Add(MouseButtonName.ScrollTiltRight.ToString(), Strings.ScrollTiltRCaption);
            ButtonCaptions.Add(MouseButtonName.SideButtonForward.ToString(), Strings.SideButtonFCaption);
            ButtonCaptions.Add(MouseButtonName.SideButtonBack.ToString(), Strings.SideButtonBCaption);

            txtMessage.Text = Strings.ButtonCustomizeMessage;
            //txtRestore.Text = _vm.SelectedApp == "AllApp" ? Strings.RestoreToDefaultActions : Strings.ButtonCustomizeRestoreCaption;
            txtSuggestedActions.Text = Strings.SuggestedActionsCaption;
            txtProductivityActions.Text = Strings.ProductivityActionsCaption;
            txtWindowsActions.Text = Strings.WindowsActionsCaption;
            txtMultimediaActions.Text = Strings.MultimediaActionsCaption;
            txtOfficeActions.Text = Strings.AdvancedActionsCaption;
            txtSearchResult.Text = Strings.SearchResultsCaption;
        }

        public void Initialize()
        {
            _vm.RefreshButtonInfo();
            txtSearchText.Text = "";
            SectionAction.Visibility = Visibility.Visible;
            SectionOffice.Visibility = Visibility.Collapsed;
            //_vm.AppSelectedIndex = "0";
            txtCaption.Focus();
            if (_vm.SelectedButton == "")
            {
                txtCaption.Text = Strings.ButtonCustomizeCaption;
                imgBack.Visibility = Visibility.Collapsed;
                Section1.Visibility = Visibility.Visible;
                SelectedActionID = -1;
            }
            else
            {
                Section1.Visibility = Visibility.Collapsed;
                SelectedActionID = _vm.SelectedActionID;
                //RefreshAction();
                LoadButtonInfo();
            }
        }

        private void LoadButtonInfo()
        {
            //SelectedActionID = _vm.SelectedActionID;

            txtCaption.Text = $"{Strings.Customize} {ButtonCaptions[_vm.SelectedButton]}";
            txtCaption.FontSize = _vm.SelectedButton == MouseButtonName.SideButtonForward.ToString() ? 18 : 20;
            imgBack.Visibility = Visibility.Visible;
            Section2.Visibility = Visibility.Visible;

            if (_vm.SelectedApp == "AllApp")
            {
                SectionAction.Visibility = Visibility.Visible;
                SectionOffice.Visibility = Visibility.Collapsed;
                RefreshAction();

                if (_vm.SuggestedActions.Contains(SelectedActionID))
                {
                    RefreshAction("Suggested");
                    var sections = GetActionSection(SelectedActionID);
                    if (sections.Length > 1)
                    { RefreshAction(sections[1]); }
                    if (ActiveActionSection != "Suggested")
                        OpenSectionPanel("SuggestedPanel", true);

                    ActiveActionSection = "Suggested";
                }
                else
                {
                    var cat = ActionCategory.None;
                    if (_vm.SelectedActionID != -1)
                        cat = Actions.KnMActions[SelectedActionID].Category!.Value;
                    if (cat == ActionCategory.None)
                    {
                        if (ActiveActionSection != "")
                        {
                            CloseSectionPanel($"{ActiveActionSection}Panel", true);
                        }
                        ActiveActionSection = "";
                    }
                    else
                    {
                        var section = cat.ToString().Replace("Action", "") ?? "";
                        if (section != "")
                        {
                            RefreshAction(section);
                            OpenSectionPanel($"{section}Panel", true);
                        }
                    }
                }
            }
            else
            {
                SectionAction.Visibility = Visibility.Collapsed;
                SectionOffice.Visibility = Visibility.Visible;
                RefreshAction("Office");
                OpenSectionPanel(OfficePanel.Name, true);
            }

            //switch(_vm.SelectedApp) {
            //  case "AllApp":
            //    break;
            //}
        }

        private void GoBackClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _vm.RefreshButtonImageFile(_vm.SelectedButton);
            _vm.SelectedButton = "";
            Initialize();
        }

        //private void SelectedApp_TextChanged(object sender, TextChangedEventArgs e) {
        //  if(_vm.SelectedButton == "") { return; }

        //  if(_vm.SelectedApp == "AllApp") {
        //    SectionAction.Visibility = Visibility.Visible;
        //    SectionOffice.Visibility = Visibility.Collapsed;
        //    Initialize();
        //  }
        //  else {
        //    RefreshAction("Office");
        //    SectionAction.Visibility = Visibility.Collapsed;
        //    SectionOffice.Visibility = Visibility.Visible;
        //    OpenSectionPanel(OfficePanel.ID, true);
        //  }
        //}

        private List<int> FilterdActions = new();
        private string searchText = "";

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchText.Text.Trim() == "")
            { txtSearchText.Text = ""; }
            if (txtSearchText.Text == "")
            {
                if (_vm.SelectedApp == "AllApp")
                {
                    SectionAction.Visibility = Visibility.Visible;
                }
                else
                {
                    SectionOffice.Visibility = Visibility.Visible;
                }
                //bdrSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                SectionAction.Visibility = Visibility.Collapsed;
                SectionOffice.Visibility = Visibility.Collapsed;
                List<int> sourceList;
                List<int> filterdList = new();
                if (txtSearchText.Text.Length > 1 && txtSearchText.Text.Length > searchText.Length)
                {
                    sourceList = FilterdActions;
                }
                else
                {
                    sourceList = _vm.SelectedApp switch
                    {
                        "AllApp" => Actions.AllActionsKnM,
                        "Word" => Actions.WordActions,
                        "Excel" => Actions.ExcelActions,
                        "PowerPoint" => Actions.PowerPointActions,
                        _ => Actions.OutlookActions
                    };
                }
                searchText = txtSearchText.Text;
                sourceList.ForEach(x =>
                {
                    if (x < 100 && Actions.KnMActions[x].Caption.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        filterdList.Add(x);
                    }
                    if (x > 100 && Actions.OfficeActions[x].Caption.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        filterdList.Add(x);
                    }
                });
                FilterdActions = filterdList;
                SearchItems.ItemsSource = null;
                SearchItems.ItemsSource = FilterdActions;
                bdrSearch.Visibility = Visibility.Visible;
            }
        }

        private void btnRestoreClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //RestoreModalDialog restoreModalDialog = new();
            //Window parentWindow = Window.GetWindow(this);
            //if (parentWindow != null)
            //{
            //    restoreModalDialog.Owner = parentWindow;
            //}

            //bool? dialogResult = restoreModalDialog.ShowDialog();
            //if (dialogResult == true)
            //{
            //    _vm!.RestoreToDefault();
            //}
            _vm!.RestoreToDefault();
        }

        private void UnfocusSearchBox(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtCaption.Focus();
        }

        private void SectionBorderClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtCaption.Focus();
            if (sender is Border border)
            {
                if (border.ActualHeight > 60)
                { return; }

                var section = border.Name.Replace("bdr", "");
                if (ActiveActionSection != section)
                {
                    OpenSectionPanel($"{section}Panel");
                    border.Cursor = System.Windows.Input.Cursors.Arrow;
                }
            }
        }

        private void SectionBorderMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                //Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
                border.Cursor = border.ActualHeight < 60 ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.Arrow;
            }
        }

        private void SectionBorderMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Border)sender).Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void ActionRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var rb = (UXRadioButton)sender;
            var id = int.Parse(rb.Name.Replace("Radio", "").Replace("_A", ""));
            if (id == SelectedActionID)
            { return; }

            var section = GetActionSection(id);
            var parameter = "";
            AdvancedAction action;
            if (Actions.AdvancedActions.Contains(id))
            {
                action = id switch
                {
                    14 => AdvancedAction.AssignKeystroke,
                    25 => AdvancedAction.OpenFile,
                    26 => AdvancedAction.OpenFolder,
                    28 => AdvancedAction.OpenWebPage,
                    _ => throw new Exception()
                };

                Window parentWindow = Window.GetWindow(this);
                double windowLeft = 0;
                double windowTop = 0;
                ActionParameterModalDialog modalDialog = new(action, parentWindow.ActualWidth, parentWindow.ActualHeight);
                if (parentWindow != null)
                {
                    modalDialog.Owner = parentWindow;
                    windowLeft = parentWindow.Left;
                    windowTop = parentWindow.Top;
                }
                modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                modalDialog.Left = windowLeft;
                modalDialog.Top = windowTop;

                if (action == AdvancedAction.AssignKeystroke)
                {
                    Task<bool> task = DdpmCommonHelper.DeviceManagerSA!.StartMouseKeystrokeRecording(_vm.CurrentDeviceID.ToString());
                    _ = task.Result;
                }
                if (modalDialog.ShowDialog()!.Value)
                {
                    Task<bool> task1 = DdpmCommonHelper.DeviceManagerSA!.StopMouseKeystrokeRecording(_vm.CurrentDeviceID.ToString());
                    _ = task1.Result;
                    Task<string> task2 = DdpmCommonHelper.DeviceManagerSA!.GetMouseKeystrokeDisplayData(_vm.CurrentDeviceID.ToString());
                    var keystroke = task2.Result;
                    parameter = modalDialog.Parameter;
                }
                else
                {
                    Task<bool> task1 = DdpmCommonHelper.DeviceManagerSA!.StopMouseKeystrokeRecording(_vm.CurrentDeviceID.ToString());
                    _ = task1.Result;
                    Initialize();
                    return;
                }
            }

            _vm.UpdateAction(id, parameter);
            RefreshAction(section[0]);
            if (section.Length > 1)
            {
                RefreshAction(section[1]);
            }
            var sectionOld = GetActionSection(SelectedActionID);
            if (sectionOld[0] != section[0] && sectionOld[0] != "")
            {
                RefreshAction(sectionOld[0]);
                if (sectionOld.Length > 1)
                {
                    RefreshAction(sectionOld[1]);
                }
            }
            SelectedActionID = id;
            if (id == 0 || SectionAction.Visibility == Visibility.Collapsed)
                Initialize();
        }

        private void EditActionClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var id = int.Parse(((ActionButton)sender).Name.Replace("btn", ""));
            var parameter = _vm.SelectedMouseAction!.AssignedAction.Parameter;
            AdvancedAction action = id switch
            {
                14 => AdvancedAction.AssignKeystroke,
                25 => AdvancedAction.OpenFile,
                26 => AdvancedAction.OpenFolder,
                28 => AdvancedAction.OpenWebPage,
                _ => throw new Exception()
            };

            Window parentWindow = Window.GetWindow(this);
            double windowLeft = 0;
            double windowTop = 0;
            ActionParameterModalDialog modalDialog = new(action, parentWindow.ActualWidth, parentWindow.ActualHeight, parameter);
            if (parentWindow != null)
            {
                modalDialog.Owner = parentWindow;
                windowLeft = parentWindow.Left;
                windowTop = parentWindow.Top;
            }
            modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
            modalDialog.Left = windowLeft;
            modalDialog.Top = windowTop;
            if (action == AdvancedAction.AssignKeystroke)
            { DdpmCommonHelper.DeviceManagerSA!.StartMouseKeystrokeRecording(_vm.CurrentDeviceID.ToString()); }

            if (modalDialog.ShowDialog()!.Value && modalDialog.Parameter != parameter)
            {
                _vm.UpdateAction(_vm.SelectedActionID, modalDialog.Parameter);
            }
        }

        private void RemoveActionClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _vm.SelectedMouseAction!.AssignedAction = new AssignedAction(_vm.SelectedMouseAction.DefaultActionID);
            _vm.RefreshButtonImageFile(_vm.SelectedButton);
            _vm.UpdateAction(_vm.SelectedMouseAction.DefaultActionID);

            var button = sender as ActionButton;
            var parent = Utility.FindParent<ItemsControl>(button!);
            //if(parent != null) { ((FrameworkElement)parent.ItemContainerGenerator.ContainerFromIndex(0)).BringIntoView(); }
            if (parent != null)
            {
                var container = (FrameworkElement)parent.ItemContainerGenerator.ContainerFromIndex(0);

                var parentObject = Utility.FindParent<UXScrollViewer>(parent);
                if (parentObject is UXScrollViewer sv)
                {
                    GeneralTransform transform = container.TransformToAncestor(parent);
                    Point point = transform.Transform(new Point(0, 0));
                    sv.ScrollToVerticalOffset(point.Y);
                }
            }
            CloseSectionPanel($"{ActiveActionSection}Panel", true);
            Initialize();
        }

        private string[] GetActionSection(int actionID)
        {
            if (_vm.SuggestedActions.Contains(actionID))
            {
                var section = Actions.KnMActions[actionID].Category?.ToString().Replace("Action", "").Replace("None", "");
                return new string[] { "Suggested", section! };
            }
            else if (Actions.KnMActions.ContainsKey(actionID))
                return new string[] { Actions.KnMActions[actionID].Category?.ToString().Replace("Action", "").Replace("None", "") ?? "" };
            else
                return new string[] { "" };
        }

        private void ActionButtonLoaded(object sender, RoutedEventArgs e)
        {
            int id;
            if (sender is UXRadioButton rb)
            {
                id = (int)((UXRadioButton)sender).DataContext;
                rb.Name = $"Radio{id}";
                rb.Content = id > 100 ? Actions.OfficeActions[id].Caption : Actions.KnMActions[id].Caption;
                if (rb.Tag.ToString() != "search")
                    rb.IsChecked = id == SelectedActionID;
            }
            else if (sender is ActionButton btn)
            {
                id = (int)((ActionButton)sender).DataContext;
                if (btn.Tag.ToString() == "Edit")
                {
                    btn.Name = $"btn{id}";
                    btn.Caption = Strings.Edit;
                    if (Actions.AdvancedActions.Contains(id))
                    {
                        btn.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btn.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    btn.Caption = Strings.Remove;
                    if (Actions.AdvancedActions.Contains(id))
                    {
                        btn.Width = 102.5;
                    }
                }
            }
            else if (sender is StackPanel sp)
            {
                id = (int)((StackPanel)sender).DataContext;
                sp.Visibility = id == SelectedActionID && id != _vm.SelectedMouseAction!.DefaultActionID ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private DockPanel? AnimatedPanel;

        private void OpenSectionPanel(string panelName, bool isFromKeyClick = false)
        {
            var section = panelName.Replace("Panel", "");
            AnimatedPanel = (DockPanel)FindName(panelName);
            AnimatedPanel.Visibility = Visibility.Visible;

            DoubleAnimation visibilityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            if (isFromKeyClick)
            { visibilityAnimation.Completed += SectionOpened; }

            AnimatedPanel.BeginAnimation(DockPanel.OpacityProperty, visibilityAnimation);

            var img = (Image)FindName($"img{section}");
            img.RenderTransform = new RotateTransform();
            DoubleAnimation rotateAnimation = new()
            {
                From = 0,
                To = 180,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            };
            img.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            if (ActiveActionSection != "" && ActiveActionSection != "Office")
                CloseSectionPanel($"{ActiveActionSection}Panel");

            ActiveActionSection = section;
        }

        private void CloseSectionPanel(string panelName, bool isAuto = false)
        {
            var section = panelName.Replace("Panel", "");
            AnimatedPanel = (DockPanel)FindName(panelName);
            //DoubleAnimation visibilityAnimation = new() {
            //  From = 1,
            //  To = 0,
            //  Duration = new Duration(TimeSpan.FromSeconds(0.3))
            //};
            //visibilityAnimation.Completed += SectionOpened;
            //AnimatedPanel.BeginAnimation(DockPanel.OpacityProperty, visibilityAnimation);
            AnimatedPanel!.Visibility = Visibility.Collapsed;
            if (isAuto)
            { ScrollAction(section, 0); }

            var img = (Image)FindName($"img{section}");
            img.RenderTransform = new RotateTransform();
            DoubleAnimation rotateAnimation = new()
            {
                From = 180,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            };
            img.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }

        private void SectionOpened(object? sender, EventArgs e)
        {
            //AnimatedPanel.Height = double.NaN;
            ScrollAction();
        }

        private void ScrollAction(string section = "", double offset = -1)
        {
            if (section == "")
            { section = ActiveActionSection; }
            if (offset == -1)
            {
                int index = 0;
                switch (ActiveActionSection)
                {
                    case "Productivity":
                        index = _vm.ProductivityActions.IndexOf(SelectedActionID);
                        break;

                    case "Windows":
                        index = _vm.WindowsActions.IndexOf(SelectedActionID);
                        break;

                    case "Multimedia":
                        index = _vm.MultimediaActions.IndexOf(SelectedActionID);
                        break;

                    case "Office":
                        index = _vm.MultimediaActions.IndexOf(SelectedActionID);
                        index = _vm.SelectedApp switch
                        {
                            "Word" => _vm.WordActions.IndexOf(_vm.SelectedActionID),
                            "Excel" => _vm.ExcelActions.IndexOf(_vm.SelectedActionID),
                            "PowerPoint" => _vm.PowerPointActions.IndexOf(_vm.SelectedActionID),
                            _ => _vm.OutlookActions.IndexOf(_vm.SelectedActionID),
                        };
                        break;
                }
                offset = index * 29;
            }
            //var itemsControl = (ItemsControl)FindName($"{ActiveActionSection}Items");
            var viewer = (UXScrollViewer)FindName($"sv{section}");
            //var container = (FrameworkElement)itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
            if (viewer is UXScrollViewer sv)
            {
                //GeneralTransform transform = container.TransformToAncestor(itemsControl);
                //Point point = transform.Transform(new Point(0, index * 29));
                //sv.ScrollToVerticalOffset(point.Y);
                sv.ScrollToVerticalOffset(offset);
            }
        }

        private void SectionButtonClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var img = (System.Windows.Controls.Image)sender;
            var section = img.Name.Replace("img", "");
            txtCaption.Focus();
            if (section != ActiveActionSection)
            { return; }

            DoubleAnimation rotateAnimation;
            if (ActiveActionSection == "")
            {
                img.RenderTransform = new RotateTransform();
                rotateAnimation = new()
                {
                    From = 0,
                    To = 180,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                };
                //img.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                OpenSectionPanel($"{section}Panel");
                ActiveActionSection = section;
            }
            else if (ActiveActionSection == section)
            {
                CloseSectionPanel($"{section}Panel");
                ActiveActionSection = "";
            }
            else
            {
            }
        }

        private void RefreshAction(string section = "")
        {
            switch (section)
            {
                case "Suggested":
                    SuggestedItems.ItemsSource = null;
                    SuggestedItems.ItemsSource = _vm.SuggestedActions;
                    break;

                case "Productivity":
                    ProductivityItems.ItemsSource = null;
                    ProductivityItems.ItemsSource = _vm.ProductivityActions;
                    break;

                case "Windows":
                    WindowsItems.ItemsSource = null;
                    WindowsItems.ItemsSource = _vm.WindowsActions;
                    break;

                case "Multimedia":
                    MultimediaItems.ItemsSource = null;
                    MultimediaItems.ItemsSource = _vm.MultimediaActions;
                    break;

                case "Office":
                    OfficeItems.ItemsSource = null;
                    OfficeItems.ItemsSource = _vm.SelectedApp switch
                    {
                        "Word" => _vm.WordActions,
                        "Excel" => _vm.ExcelActions,
                        "PowerPoint" => _vm.PowerPointActions,
                        _ => _vm.OutlookActions
                    };
                    break;

                default:
                    SuggestedItems.ItemsSource = null;
                    ProductivityItems.ItemsSource = null;
                    WindowsItems.ItemsSource = null;
                    MultimediaItems.ItemsSource = null;
                    SuggestedItems.ItemsSource = _vm.SuggestedActions;
                    ProductivityItems.ItemsSource = _vm.ProductivityActions;
                    WindowsItems.ItemsSource = _vm.WindowsActions;
                    MultimediaItems.ItemsSource = _vm.MultimediaActions;
                    CloseSectionPanel(SuggestedPanel.Name);
                    CloseSectionPanel(ProductivityPanel.Name);
                    CloseSectionPanel(WindowsPanel.Name);
                    CloseSectionPanel(MultimediaPanel.Name);
                    ActiveActionSection = "";
                    break;
            }
        }
    }
}