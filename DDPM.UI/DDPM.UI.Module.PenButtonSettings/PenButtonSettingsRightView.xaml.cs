using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.PenButtonSettings
{
    /// <summary>
    /// Interaction logic for PenButtonSettingsRightView.xaml
    /// </summary>
    public partial class PenButtonSettingsRightView : UserControl
    {
        private readonly PenViewModel _vm;

        private readonly Dictionary<string, string> ButtonCaptions = new();
        private string ActiveActionSection = "";

        public PenButtonSettingsRightView(PenViewModel vm)
        {
            try
            {
                InitializeComponent();
                _vm = vm;

                ButtonCaptions.Add(PenButtonName.TopButton.ToString(), Strings.TopButtonCaption);
                ButtonCaptions.Add(PenButtonName.TopBarrelButton.ToString(), Strings.TopBarrelButtonCaption);
                ButtonCaptions.Add(PenButtonName.BottomBarrelButton.ToString(), Strings.BottomBarrelButtonCaption);

                txtClickOnce.Text = Strings.PenButtonClickOnce;
                txtDoubleClick.Text = Strings.PenButtonDoubleClick;
                txtPressAndHold.Text = Strings.PenButtonPressHold;
                txtHoverClick.Text = Strings.HoverClick;

                txtMessage.Text = Strings.PenButtonCustomizeMessage;
                txtRestore.Text = Strings.PenButtonCustomizeRestoreCaption;
                txtSuggestedActions.Text = Strings.SuggestedActionsCaption;
                txtProductivityActions.Text = Strings.ProductivityActionsCaption;
                txtWindowsActions.Text = Strings.WindowsActionsCaption;
                txtMultimediaActions.Text = Strings.MultimediaActionsCaption;
                txtSearchResult.Text = Strings.SearchResultsCaption;
                txtSearchText.Watermark = LangHelper.Instance["SearchActions"];
                ParentBorder.SizeChanged += ParentBorder_SizeChanged;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs  PenButtonSettingsRightView() ex:" + ex.Message);
            }
        }

        private void ParentBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetDockPanel();
        }

        public void Initialize()
        {

            try
            {
                CloseBehaviorSection();
                _vm.SelectedBehavior = "";
                _vm.RefreshButtonInfo();
                txtSearchText.Text = "";
                txtCaption.Focus();
                if (_vm.SelectedButton == "")
                {
                    txtCaption.Text = Strings.ButtonCustomizeCaption;
                    imgBack.Visibility = Visibility.Collapsed;
                    Section1.Visibility = Visibility.Visible;
                }
                else
                {
                    Section1.Visibility = Visibility.Collapsed;
                    LoadButtonInfo();
                    if (_vm.SelectedButton == PenButtonName.TopButton.ToString())
                    {
                        HoverClick.Visibility = Visibility.Collapsed;
                        bdrClickOnce.Visibility = Visibility.Visible;
                        bdrDoubleClick.Visibility = Visibility.Visible;
                        bdrPressAndHold.Visibility = Visibility.Visible;
                        SectionAction.Visibility = Visibility.Collapsed;
                        bdrSuggested.Width = 272;
                        bdrProductivity.Width = 272;
                        bdrWindows.Width = 272;
                        bdrMultimedia.Width = 272;
                    }
                    else
                    {
                        HoverClick.Visibility = Visibility.Visible;
                        bdrClickOnce.Visibility = Visibility.Collapsed;
                        bdrDoubleClick.Visibility = Visibility.Collapsed;
                        bdrPressAndHold.Visibility = Visibility.Collapsed;
                        SectionAction.Visibility = Visibility.Visible;
                        bdrSuggested.Width = 288;
                        bdrProductivity.Width = 288;
                        bdrWindows.Width = 288;
                        bdrMultimedia.Width = 288;
                        ShowAction();
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs  Initialize() ex:" + ex.Message);
            }
        }

        private void LoadButtonInfo()
        {
            try
            {
                txtCaption.Text = $"{Strings.Customize} {ButtonCaptions[_vm.SelectedButton]}";
                txtCaption.FontSize = _vm.SelectedButton == PenButtonName.BottomBarrelButton.ToString() ? 18 : 20;
                imgBack.Visibility = Visibility.Visible;
                Section2.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs  LoadButtonInfo() ex:" + ex.Message);
            }
        }

        private void SetDockPanel()
        {
            try
            {
                if (_vm.SelectedButton == PenButtonName.TopButton.ToString())
                {
                    ProductivityPanel.MaxHeight = _vm.ProductivityActionsTopButton.Contains(_vm.SelectedActionID) ? 202 : 155;
                    WindowsPanel.Height = _vm.SelectedBehavior switch
                    {
                        "ClickOnce" => _vm.WindowsActionsTopButton.Contains(_vm.SelectedActionID) && _vm.SelectedActionID != 73 ? ParentBorder.ActualHeight - 405 : ParentBorder.ActualHeight - 453,
                        "DoubleClick" => _vm.WindowsActionsTopButton.Contains(_vm.SelectedActionID) && _vm.SelectedActionID != 90 ? ParentBorder.ActualHeight - 438 : ParentBorder.ActualHeight - 454,
                        _ => ParentBorder.ActualHeight - 481,
                    };
                    MultimediaPanel.Height = _vm.SelectedBehavior switch
                    {
                        "ClickOnce" => ParentBorder.ActualHeight - 466,
                        "DoubleClick" => ParentBorder.ActualHeight - 509,
                        _ => ParentBorder.ActualHeight > 552 ? ParentBorder.ActualHeight - 552 : 0,
                    };
                }
                else
                {
                    ProductivityPanel.MaxHeight = 800;
                    WindowsPanel.Height = ParentBorder.ActualHeight == 0 ? 0 : ParentBorder.ActualHeight - 401;
                    MultimediaPanel.Height = ParentBorder.ActualHeight == 0 ? 0 : ParentBorder.ActualHeight - 472;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs  SetDockPanel() ex:" + ex.Message);
            }
        }

        private List<int> FilterdActions = new();
        private string searchText = "";

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtSearchText.Text.Trim() == "")
                { txtSearchText.Text = ""; }
                if (txtSearchText.Text == "")
                {
                    SectionAction2.Visibility = Visibility.Visible;
                    bdrSearch.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SectionAction2.Visibility = Visibility.Collapsed;
                    bdrSearch.Visibility = Visibility.Visible;

                    List<int> sourceList;
                    List<int> filterdList = new();
                    if (txtSearchText.Text.Length > 1 && txtSearchText.Text.Length > searchText.Length)
                    {
                        sourceList = FilterdActions;
                    }
                    else
                    {
                        sourceList = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? Actions.AllActionsPenTopButton() : Actions.AllActionsPenBarrelButton();
                    }
                    searchText = txtSearchText.Text;
                    sourceList.ForEach(x =>
                    {
                        if (Actions.PenActions[x].Caption.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                        {
                            filterdList.Add(x);
                        }
                    });
                    FilterdActions = filterdList;
                    SearchItems.ItemsSource = null;
                    SearchItems.ItemsSource = FilterdActions;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs  SearchBox_TextChanged() ex:" + ex.Message);
            }
        }

        private void Behavior_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border bdr)
                {
                    CloseBehaviorSection();
                    var behavior = bdr.Name.Replace("bdr", "");
                    if (behavior == _vm.SelectedBehavior)
                    {
                        _vm.SelectedBehavior = "";
                        return;
                    }

                    var children = Behaviors.Children;
                    var child = children[1];
                    if (children[1] is StackPanel)
                        children.RemoveAt(1);
                    else if (children[2] is StackPanel)
                    {
                        child = children[2];
                        children.RemoveAt(2);
                    }
                    else
                    {
                        child = children[3];
                        children.RemoveAt(3);
                    }

                    switch (behavior)
                    {
                        case "ClickOnce":
                            children.Insert(1, child);
                            break;

                        case "DoubleClick":
                            children.Insert(2, child);
                            break;

                        case "PressAndHold":
                            children.Insert(3, child);
                            break;
                    }

                    _vm.SelectedBehavior = behavior;
                    OpenBehaviorSection();
                    ShowAction();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs  Behavior_MouseLeftButtonDown ex:" + ex.Message);
            }
        }

        private void ShowAction()
        {
            try
            {
                SetDockPanel();
                RefreshAction();
                if ((_vm.SelectedButton == PenButtonName.TopButton.ToString() && _vm.SuggestedActionsTopButton.Contains(_vm.SelectedActionID)) ||
                   (_vm.SelectedButton != PenButtonName.TopButton.ToString() && _vm.SuggestedActionsBarrelButton.Contains(_vm.SelectedActionID)))
                {
                    RefreshAction("Suggested");
                    //var sections = GetActionSection(_vm.SelectedActionID);
                    if (ActiveActionSection != "Suggested")
                        OpenSectionPanel("SuggestedPanel", true);

                    ActiveActionSection = "Suggested";
                }
                else
                {
                    var cat = ActionCategory.None;
                    if (_vm.SelectedActionID != -1)
                        //cat = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? Actions.PenActions[_vm.SelectedActionID].Category!.Value : Actions.KnMActions[_vm.SelectedActionID].Category!.Value;
                        cat = Actions.PenActions[_vm.SelectedActionID].Category!.Value;
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
                            OpenSectionPanel($"{section}Panel", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs  ShowAction ex:" + ex.Message);
            }
        }

        private void ArrangeActionSection(string behavior)
        {
            try
            {
                var children = Behaviors.Children;
                var child = children[1];
                if (children[1] is StackPanel)
                    children.RemoveAt(1);
                else if (children[2] is StackPanel)
                {
                    child = children[2];
                    children.RemoveAt(2);
                }
                else
                {
                    child = children[3];
                    children.RemoveAt(3);
                }

                switch (behavior)
                {
                    case "ClickOnce":
                        children.Insert(1, child);
                        break;

                    case "DoubleClick":
                        children.Insert(2, child);
                        break;

                    case "PressAndHold":
                        children.Insert(3, child);
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs ArrangeActionSection ex:" + ex.Message);
            }
        }

        private void ActionButtonLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //int id;
                if (sender is UXRadioButton rb)
                {
                    if (rb.DataContext is int id)
                    {
                        rb.Name = $"Radio{id}";
                        //rb.Content = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? Actions.PenActions[id].Caption : Actions.KnMActions[id].Caption;
                        rb.Content = Actions.PenActions[id].Caption;
                        //rb.Content = _vm.ActionNames[id];
                        //if (rb.Tag.ToString() != "search")
                        rb.IsChecked = id == _vm.SelectedActionID;
                    }
                }
                else if (sender is ActionButton btn)
                {
                    if (btn.DataContext is int id)
                    {
                        if (btn.Tag.ToString() == "Edit")
                        {
                            btn.Name = $"btn{id}";
                            btn.Caption = Strings.Edit;
                            if (Actions.AdvancedActionsPen.Contains(id))
                            {
                                btn.Visibility = Visibility.Visible;
                                btn.Width = 101;
                            }
                            else
                            {
                                btn.Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            btn.Caption = Strings.Remove;
                            if (Actions.AdvancedActionsPen.Contains(id))
                            {
                                btn.Width = 101;
                            }
                        }
                    }
                }
                else if (sender is StackPanel sp)
                {
                    if (sp.DataContext is int id)
                    {
                        sp.Visibility = id == _vm.SelectedActionID && id != _vm.SelectedAction!.DefaultActionID ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs ActionButtonLoaded ex:" + ex.Message);
            }
        }

        private void ActionRadioButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rb = (UXRadioButton)sender;
                var id = int.Parse(rb.Name.Replace("Radio", "").Replace("_A", ""));
                if (id == _vm.SelectedActionID)
                { return; }

                //var section = GetActionSection(id);
                var parameter = "";
                AdvancedAction action;
                if (id == 8)
                {
                    action = AdvancedAction.AssignKeystroke;

                    Window parentWindow = Window.GetWindow(this);
                    double windowLeft = 0;
                    double windowTop = 0;
                    ActionParameterModalDialog modalDialog = new(action, parentWindow.ActualWidth, parentWindow.ActualHeight, "", "PEN");
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;

                    if (modalDialog.ShowDialog()!.Value)
                    {
                        parameter = modalDialog.Parameter;
                    }
                    else
                    {
                        txtSearchText.Clear();
                        ShowAction();
                        return;
                    }
                }
                else if (id == 23)
                {
                    Window parentWindow = Window.GetWindow(this);
                    double windowLeft = 0;
                    double windowTop = 0;
                    OpenRunModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, _vm.LaunchableAppValues);
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    if (modalDialog.ShowDialog()!.Value)
                    {
                        //parameter = $"{modalDialog.ID}|{modalDialog.Parameter}";
                        parameter = $"{modalDialog.Parameter}";
                    }
                    else
                    {
                        txtSearchText.Clear();
                        ShowAction();
                        return;
                    }
                }

                _vm.UpdateAction(id, parameter);
                txtSearchText.Clear();
                ShowAction();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs ActionRadioButton_Click ex:" + ex.Message);
            }
        }

        private void GoBackClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                _vm.RefreshButtonImageFile(_vm.SelectedButton);
                _vm.SelectedButton = "";
                Initialize();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs GoBackClicked ex:" + ex.Message);
            }
        }

        private void btnRestoreClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                RestoreModalDialog restoreModalDialog = new();
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    restoreModalDialog.Owner = parentWindow;
                }

                bool? dialogResult = restoreModalDialog.ShowDialog();
                if (dialogResult == true)
                {
                    _vm?.RestoreToDefault();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs btnRestoreClicked ex:" + ex.Message);
            }
        }

        private void UnfocusSearchBox(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtCaption.Focus();
        }

        private void SectionBorderMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (sender is Border border)
                {
                    border.Cursor = border.ActualHeight < 60 ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs SectionBorderMouseEnter ex:" + ex.Message);
            }
        }

        private void SectionBorderMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((Border)sender).Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void SectionBorderClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs SectionBorderClick ex:" + ex.Message);
            }
        }

        private void EditActionClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {

                var id = int.Parse(((ActionButton)sender).Name.Replace("btn", ""));
                var parameter = _vm.SelectedAction!.AssignedAction.Parameter;

                Window parentWindow = Window.GetWindow(this);
                double windowLeft = 0;
                double windowTop = 0;
                if (id == 8)
                {
                    ActionParameterModalDialog modalDialog = new(AdvancedAction.AssignKeystroke, parentWindow.ActualWidth, parentWindow.ActualHeight, parameter, "PEN");
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    if (modalDialog.ShowDialog()!.Value && modalDialog.Parameter != parameter)
                    {
                        _vm.UpdateAction(_vm.SelectedActionID, modalDialog.Parameter);
                    }
                }
                else if (id == 23)
                {
                    //var arr = parameter.Split('|');
                    //OpenRunModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, int.Parse(arr[0]), arr[1]);
                    OpenRunModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, _vm.LaunchableAppValues, parameter);
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    //if (modalDialog.ShowDialog()!.Value && $"{modalDialog.ID}|{modalDialog.Parameter}" != parameter)
                    //{
                    //    _vm.UpdateAction(_vm.SelectedActionID, $"{modalDialog.ID}|{modalDialog.Parameter}");
                    //}
                    if (modalDialog.ShowDialog()!.Value && $"{modalDialog.Parameter}" != parameter)
                    {
                        _vm.UpdateAction(_vm.SelectedActionID, $"{modalDialog.Parameter}");
                    }
                }
                else if (id == 41)
                {
                    RadialMenuModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, _vm);
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    modalDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs EditActionClick ex:" + ex.Message);
            }
        }

        private void RemoveActionClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                _vm.SelectedAction!.AssignedAction = new AssignedAction(_vm.SelectedAction.DefaultActionID);
                _vm.RefreshButtonImageFile(_vm.SelectedButton);
                _vm.UpdateAction(_vm.SelectedAction.DefaultActionID);

                ShowAction();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs RemoveActionClick ex:" + ex.Message);
            }
        }

        private DockPanel? AnimatedPanel;

        private void OpenSectionPanel(string panelName, bool isFromKeyClick = false)
        {
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs OpenSectionPanel ex:" + ex.Message);
            }
        }

        private void OpenBehaviorSection()
        {
            try
            {
                if (_vm.SelectedBehavior == "")
                { return; }

                var img = (Image)FindName($"img{_vm.SelectedBehavior}");
                img.RenderTransform = new RotateTransform();
                DoubleAnimation rotateAnimation = new()
                {
                    From = 0,
                    To = 180,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                };
                img.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                SectionAction.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs OpenBehaviorSection ex:" + ex.Message);
            }
        }

        private void CloseSectionPanel(string panelName, bool isAuto = false)
        {
            try
            {
                var section = panelName.Replace("Panel", "");
                AnimatedPanel = (DockPanel)FindName(panelName);
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs CloseSectionPanel ex:" + ex.Message);
            }
        }

        private void CloseBehaviorSection()
        {
            try
            {
                if (_vm.SelectedBehavior == "")
                { return; }

                var img = (Image)FindName($"img{_vm.SelectedBehavior}");
                img.RenderTransform = new RotateTransform();
                DoubleAnimation rotateAnimation = new()
                {
                    From = 180,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                };
                img.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                SectionAction.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs CloseBehaviorSection ex:" + ex.Message);
            }
        }

        private void SectionOpened(object? sender, EventArgs e)
        {
            ScrollAction();
        }

        private void ScrollAction(string section = "", double offset = -1)
        {
            try
            {
                if (section == "")
                { section = ActiveActionSection; }
                if (offset == -1)
                {
                    int index = 0;
                    switch (ActiveActionSection)
                    {
                        case "Productivity":
                            index = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.ProductivityActionsTopButton.IndexOf(_vm.SelectedActionID) : _vm.ProductivityActionsBarrelButton.IndexOf(_vm.SelectedActionID);
                            break;

                        case "Windows":
                            index = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.WindowsActionsTopButton.IndexOf(_vm.SelectedActionID) : _vm.WindowsActionsBarrelButton.IndexOf(_vm.SelectedActionID);
                            break;

                        case "Multimedia":
                            index = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.MultimediaActionsTopButton.IndexOf(_vm.SelectedActionID) : _vm.MultimediaActionsBarrelButton.IndexOf(_vm.SelectedActionID);
                            break;

                        default:
                            break;
                    }
                    offset = index * 29;
                }
                var viewer = (UXScrollViewer)FindName($"sv{section}");
                if (viewer is UXScrollViewer sv)
                {
                    sv.ScrollToVerticalOffset(offset);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs ScrollAction ex:" + ex.Message);
            }
        }

        private void SectionButtonClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs SectionButtonClick ex:" + ex.Message);
            }
        }

        private void RefreshAction(string section = "")
        {
            try
            {
                switch (section)
                {
                    case "Suggested":
                        SuggestedItems.ItemsSource = null;
                        SuggestedItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.SuggestedActionsTopButton : _vm.SuggestedActionsBarrelButton;
                        break;

                    case "Productivity":
                        ProductivityItems.ItemsSource = null;
                        ProductivityItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.ProductivityActionsTopButton : _vm.ProductivityActionsBarrelButton;
                        break;

                    case "Windows":
                        WindowsItems.ItemsSource = null;
                        WindowsItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.WindowsActionsTopButton : _vm.WindowsActionsBarrelButton;
                        break;

                    case "Multimedia":
                        MultimediaItems.ItemsSource = null;
                        MultimediaItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.MultimediaActionsTopButton : _vm.MultimediaActionsBarrelButton;
                        break;

                    default:
                        SuggestedItems.ItemsSource = null;
                        ProductivityItems.ItemsSource = null;
                        WindowsItems.ItemsSource = null;
                        MultimediaItems.ItemsSource = null;
                        SuggestedItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.SuggestedActionsTopButton : _vm.SuggestedActionsBarrelButton;
                        ProductivityItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.ProductivityActionsTopButton : _vm.ProductivityActionsBarrelButton;
                        WindowsItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.WindowsActionsTopButton : _vm.WindowsActionsBarrelButton;
                        MultimediaItems.ItemsSource = _vm.SelectedButton == PenButtonName.TopButton.ToString() ? _vm.MultimediaActionsTopButton : _vm.MultimediaActionsBarrelButton;
                        CloseSectionPanel(SuggestedPanel.Name);
                        CloseSectionPanel(ProductivityPanel.Name);
                        CloseSectionPanel(WindowsPanel.Name);
                        CloseSectionPanel(MultimediaPanel.Name);
                        ActiveActionSection = "";
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs SectionButtonClick ex:" + ex.Message);
            }
        }

        private void txtSearchText_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                txtSearchText.Clear();
                e.Handled = true;
            }
        }

        private void txtSearchText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !_vm.CheckChar(e.Text);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Module.PenSettings\\PenSettingsRightView.xaml.cs txtSearchText_PreviewTextInput ex:" + ex.Message);
            }
        }
    }
}