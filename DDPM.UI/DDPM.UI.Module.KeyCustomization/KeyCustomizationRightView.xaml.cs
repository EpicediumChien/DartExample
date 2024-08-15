using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DDPM.UI.Module.KeyCustomization
{
    /// <summary>
    /// Interaction logic for KeyCustomizationRightView.xaml
    /// </summary>
    public partial class KeyCustomizationRightView : System.Windows.Controls.UserControl
    {
        private readonly KeyboardViewModel _vm;

        private int SelectedActionID = -1;

        public KeyCustomizationRightView(KeyboardViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            txtMessage.Text = Strings.KeyCustomizeMessage;
            txtRestore.Text = Strings.KeyCustomizeRestoreCaption;
            txtSuggestedActions.Text = Strings.SuggestedActionsCaption;
            txtProductivityActions.Text = Strings.ProductivityActionsCaption;
            txtWindowsActions.Text = Strings.WindowsActionsCaption;
            txtMultimediaActions.Text = Strings.MultimediaActionsCaption;
            txtSearchResult.Text = Strings.SearchResultsCaption;
        }

        public void Initialize()
        {
            txtSearchText.Text = "";
            SectionAction.Visibility = Visibility.Visible;
            txtCaption.Focus();
            if (_vm.SelectedKey == "")
            {
                txtCaption.Text = Strings.KeyCustomizeCaptionCaption;
                imgBack.Visibility = Visibility.Collapsed;
                Section1.Visibility = Visibility.Visible;
                SelectedActionID = -1;
            }
            else
            {
                Section1.Visibility = Visibility.Collapsed;
                SelectedActionID = _vm.SelectedActionID;
                RefreshAction();
                LoadKeyInfo();
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

        private void LoadKeyInfo()
        {
            txtCaption.Text = $"{Strings.Customize} {_vm.SelectedKey}";
            imgBack.Visibility = Visibility.Visible;
            Section2.Visibility = Visibility.Visible;

            if (_vm.SuggestedActions.Contains(SelectedActionID))
            {
                RefreshAction("Suggested");
                var sections = GetActionSection(SelectedActionID);
                if (sections.Length > 1) { RefreshAction(sections[1]); }
                if (ActiveActionSection != "Suggested")
                    OpenSectionPanel("SuggestedPanel", true);

                ActiveActionSection = "Suggested";
            }
            else
            {
                var cat = Actions.AllActions[SelectedActionID].Category;
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
                    var section = cat?.ToString().Replace("Action", "") ?? "";
                    if (section != "")
                    {
                        RefreshAction(section);
                        OpenSectionPanel($"{section}Panel", true);
                    }
                }
            }
        }

        private void GoBackClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _vm.RefreshKeyImageFile(_vm.SelectedKey);
            _vm.SelectedKey = "";
            Initialize();
        }

        private void btnRestoreClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                _vm!.RestoreToDefault();
            }
        }

        private List<int> FilterdActions = new();
        private string searchText = "";

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchText.Text.Trim() == "") { txtSearchText.Text = ""; }
            if (txtSearchText.Text == "")
            {
                SectionAction.Visibility = Visibility.Visible;
                //bdrSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                SectionAction.Visibility = Visibility.Collapsed;
                List<int> sourceList;
                List<int> filterdList = new();
                if (txtSearchText.Text.Length > 1 && txtSearchText.Text.Length > searchText.Length)
                {
                    sourceList = FilterdActions;
                }
                else
                {
                    sourceList = Actions.AllActionsKnM;
                }
                searchText = txtSearchText.Text;
                sourceList.ForEach(x =>
                {
                    if (Actions.AllActions[x].Caption.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
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

        private void ActionRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var rb = (UXRadioButton)sender;
            var id = int.Parse(rb.Name.Replace("Radio", "").Replace("_A", ""));
            if (id == SelectedActionID) { return; }

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
                if (modalDialog.ShowDialog()!.Value)
                {
                    parameter = modalDialog.Parameter;
                }
                else
                {
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
            if (SelectedActionID == 0 || SectionAction.Visibility == Visibility.Collapsed)
                Initialize();
        }

        private void EditActionClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var id = int.Parse(((ActionButton)sender).Name.Replace("btn", ""));
            var parameter = _vm.SelectedAction!.AssignedAction.Parameter;
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
            if (modalDialog.ShowDialog()!.Value && modalDialog.Parameter != parameter)
            {
                _vm.UpdateAction(_vm.SelectedActionID, modalDialog.Parameter);
            }
        }

        private void RemoveActionClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _vm.SelectedAction!.AssignedAction = new AssignedAction(_vm.SelectedAction.DefaultActionID);
            _vm.RefreshKeyImageFile(_vm.SelectedKey);
            _vm.UpdateAction(_vm.SelectedAction.DefaultActionID);

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
                var section = Actions.AllActions[actionID].Category?.ToString().Replace("Action", "").Replace("None", "");
                return new string[] { "Suggested", section! };
            }
            else if (Actions.AllActions.ContainsKey(actionID))
                return new string[] { Actions.AllActions[actionID].Category?.ToString().Replace("Action", "").Replace("None", "") ?? "" };
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
                rb.Content = Actions.AllActions[id].Caption;
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
                sp.Visibility = id == SelectedActionID && id != _vm.SelectedAction!.DefaultActionID ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string ActiveActionSection = "";

        private void SectionBorderClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtCaption.Focus();
            if (sender is Border border)
            {
                if (border.ActualHeight > 60) { return; }

                var section = border.Name.Replace("bdr", "");
                if (ActiveActionSection != section)
                {
                    OpenSectionPanel($"{section}Panel");
                    border.Cursor = System.Windows.Input.Cursors.Arrow;
                }
            }
        }

        private void SectionButtonClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var img = (System.Windows.Controls.Image)sender;
            var section = img.Name.Replace("img", "");
            txtCaption.Focus();
            if (section != ActiveActionSection) { return; }

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
            if (isFromKeyClick) { visibilityAnimation.Completed += SectionOpened; }

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

            if (ActiveActionSection != "")
                CloseSectionPanel($"{ActiveActionSection}Panel");

            ActiveActionSection = section;
        }

        private void SectionOpened(object? sender, EventArgs e)
        {
            //AnimatedPanel.Height = double.NaN;
            ScrollAction();
        }

        private void ScrollAction(string section = "", double offset = -1)
        {
            if (section == "") { section = ActiveActionSection; }
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
                        //MultimediaItems.ItemsSource = _vm.MultimediaActions;
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
            if (isAuto) { ScrollAction(section, 0); }

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

        private void UnfocusSearchBox(object sender, MouseButtonEventArgs e)
        {
            txtCaption.Focus();
        }
    }
}