using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace DDPM.UI.Common
{
    public static partial class DdpmCommonHelper
    {
        /// <summary>
        /// Switch to light mode color define
        /// </summary>
        /// <history>
        /// Add SplitListView - EzBtn (Robert_Lin, 2024-10-24)
        /// Add regions and sort functions (Eric Chien, 2024-10-30)
        /// </history>
        public static void SwitchToLightMode()
        {
            #region Default Text Color
            UpdateFreezable("DefaultTheme_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
            UpdateFreezable("DefaultTheme_BdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
            UpdateFreezable("DefaultTheme_BdBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
            UpdateFreezable("DefaultTheme_RbBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#80FFFFFF"));
            UpdateFreezable("DefaultTheme_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("DefaultTheme_PbHeaderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("DefaultTheme_USBKVMPCColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
            UpdateFreezable("DefaultTheme_BtFgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("DefaultTheme_BtBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D9E1F2"));
            UpdateFreezable("DefaultTheme_BtBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateBitmapImage("popup_ArrowCorner", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/LightMode/popup_ArrowCorner.png", UriKind.RelativeOrAbsolute));
            UpdateFreezable("DefaultTheme_TbBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            UpdateFreezable("DefaultTheme_TbFgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
            UpdateFreezable("DefaultTheme_TbBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("DefaultTheme_ListBoxBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99E7EAEE"));
            #endregion

            #region Add Device
            UpdateFreezable("RightMenuBg_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
            UpdateFreezable("txtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("ActionsTextBlock_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("ActionsBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f6f9fc"));
            UpdateFreezable("ActionsRadio_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
            UpdateFreezable("ActionsRadio_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
            UpdateFreezable("ActionsRadio_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("SearchBoxBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
            UpdateFreezable("SearchTextBox_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
            UpdateFreezable("SearchTextBox_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#404040"));
            UpdateFreezable("SeachResultBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
            UpdateFreezable("ModelNumberText_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#185896"));
            UpdateFreezable("KeyToolTipText_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("KeyToolTipText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
            UpdateFreezable("ActionParameterModalDialog_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#e3eef5"));
            UpdateFreezable("CaptionText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("DescriptionText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("Keystroke_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("Keystroke_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable("txtOpen_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("txtOpen_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable("CancelButton_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00FFFFFF"));
            UpdateFreezable("CancelButton_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("CancelButtonBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("SaveButtonDisa_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#B6B6B6"));
            UpdateFreezable("SaveButtonDisa_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("txtActionCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateBitmapImage("title_back", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left-light.png", UriKind.RelativeOrAbsolute));
            UpdateBitmapImage("mouse_setting_title_back", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left-light.png", UriKind.RelativeOrAbsolute));
            UpdateBitmapImage("down_Expand", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Down-light.png", UriKind.RelativeOrAbsolute));
            UpdateFreezable("txtMessage_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("MouseSettingMenu_BgColor99", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99f5f8f9"));
            UpdateFreezable("MouseSettingMenu_BgColor60", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#60f5f8f9"));

            UpdateFreezable("AddDeviceTxtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("AddDeviceBorder1_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99f5f8f9"));
            UpdateFreezable("AddDeviceTextStyle1_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

            UpdateFreezable("RightView_Header_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("RightView_Header_Text_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A2343"));
            UpdateFreezable("RightView_Header_Text_HighlightColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A2343"));

            UpdateFreezable("bdrAlert_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

            UpdateFreezable("bdrAlert_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#364861"));


            UpdateFreezable("ModuleCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("ModuleItem_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

            UpdateFreezable("txtUnpair_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

            UpdateBitmapImage("Search_img", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Search_light.png", UriKind.RelativeOrAbsolute));
            UpdateBitmapImage("Close_img", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/Close_light.png", UriKind.RelativeOrAbsolute));

            UpdateFreezable("ActionParameterModalDialogTextBoxBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));

            UpdateFreezable("ActionsBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));

            UpdateFreezable("Border1BorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));

            UpdateFreezable("SearchBoxBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));

            UpdateBitmapImage("left_img", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Left_light.png", UriKind.RelativeOrAbsolute));
            #endregion

            #region dialog
            //Dialog start
            UpdateFreezable("BorderStyle1_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));

            UpdateFreezable("TextBlockStyle1_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

            UpdateFreezable("LabelStyle1_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#040404"));

            UpdateFreezable("RadialMenuModalDialogToggleSwitch_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

            UpdateFreezable("txtLabelText_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("txtLabelText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));

            UpdateFreezable("PairTileModalDialogTextStyle1FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

            UpdateFreezable("PairTileModalDialogBgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));

            UpdateFreezable("UnpairModalDialogBorder1", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

            UpdateFreezable("BorderContinueBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("BorderContinueBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E1C2F"));

            UpdateFreezable("BorderYesBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("BorderYesBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E1C2F"));

            UpdateFreezable("txtAlert_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("txtAlert_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#364861"));

            UpdateFreezable("WaitingModalDialogBorderBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E0f4f7f9"));
            UpdateFreezable("WaitingModalDialogBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

            UpdateFreezable("bdrAlertBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#616130"));
            UpdateFreezable("WaitingModalDialogBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF8040"));
            //Dialog end
            #endregion

            #region keyboard
            UpdateFreezable("RightMenuBg_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
            UpdateFreezable("txtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("ActionsTextBlock_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            UpdateFreezable("ActionsGrid_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f7fafc"));

            UpdateFreezable("ActionsBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f7fafc"));

            UpdateFreezable("SearchBoxBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));

            #endregion

            #region Main Window Background
            SplashPath = "Resources/Images/splash{0}-round_Light.png";
            UpdateFreezable("mainWindowBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFECF3F9"));
            UpdateFreezable("bkImage", (ref ImageBrush brush) => brush.ImageSource = GetImageSourceFromCommonResource("Resources/Images/Background_Light.png", "DDPM"));
            UpdateFreezable("SpinnerWaitTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("SpinnerBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            #endregion

            #region Add Device
            #endregion

            #region PathIcon
            UpdateFreezable("PathIcon_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#434646"));
            UpdateFreezable("FakeTransparent", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01000000"));
            UpdateFreezable("MouseHover_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("Window_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            #endregion

            #region HomePage
            UpdateFreezable("HomePageFeature_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#2460B4"));
            UpdateFreezable("HomePageKeyFeature_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("HomePageButton_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("HomePageButton_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            #endregion

            #region Battery
            UpdateFreezable("BatteryIndicator_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            UpdateFreezable("TextBlock_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            #endregion

            #region Display port icon
            UpdateFreezable("DisplayName_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
            UpdateBitmapImage("ConnectionTypeImageKey", new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/LightMode/Port.png", UriKind.Absolute));
            #endregion

            #region Combo Box
            UpdateFreezable("DdpmCB_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFD2D2D2"));
            UpdateFreezable("DdpmCB_BdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFE1EAF3"));
            UpdateFreezable("DdpmCB_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            UpdateFreezable("DdpmCB_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFEEEEEE"));
            UpdateFreezable("DdpmCB_SelTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF49A3E5"));
            #endregion

            #region BarItem Colors
            UpdateBitmapImage("Arrow_Left", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left-light.png", UriKind.RelativeOrAbsolute));
            UpdateFreezable("btn_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#2B6FC7"));
            #endregion

            #region VberItem1
            UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
            UpdateFreezable("DefaultTheme_USBKVMComboBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5FFFFFF"));
            UpdateFreezable("DefaultTheme_USBKVMComboBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E1E6EF"));
            UpdateFreezable("Vbar_FgBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("Vbar_BkBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
            UpdateFreezable("Vbar_BdBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
            UpdateFreezable("Vbar_FgBrush_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#9E9E9E"));
            UpdateFreezable("Vbar_TextColor_Normal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("Vbar_TextColor_Selected", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            #endregion

            #region Right View Container
            UpdateFreezable("RightView_Header_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FBFDFF"));
            UpdateFreezable("RightView_Header_Text_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("RightViewContainerBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("GroupExpanderHeaderBottomLineColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("RightView_Group_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
            UpdateFreezable("RightView_Group_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
            UpdateFreezable("RightView_Combo_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("RightView_Combo_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("RightView_Combo_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
            UpdateFreezable("RightBaseBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F1F6FA"));
            UpdateFreezable("RightView_PercentageTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
            UpdateFreezable("Brightness_TimeText_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
            UpdateFreezable("Brightness_TimeAMPM_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("RightView_Btn_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("RightView_Btn_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("RightGroupBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99F8FbFE"));
            UpdateFreezable("GroupBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F8FBFE"));
            UpdateFreezable("PartBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            #endregion

            #region Tooltip (Robert_Lin, 2024-10-24)
            UpdateFreezable("Tooltip_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("Tooltip_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E9E9E9"));
            UpdateFreezable("Tooltip_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            #endregion

            #region Button
            UpdateFreezable("Border_Btn_txt", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("CheckBox_Brush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));
            UpdateFreezable("BorderBrushColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
            UpdateFreezable("Button_UXStyleColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("Button_UXStyleTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("Button_UXStyleColor_ForWhiteFrame", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            #endregion

            #region SecondaryButton
            UpdateFreezable("SecondaryButton_DefaultBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));
            UpdateFreezable("SecondaryButton_DefaultBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("SecondaryButton_DefaultTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));

            UpdateFreezable("SecondaryButton_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D9F5FD"));
            UpdateFreezable("SecondaryButton_HoverBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("SecondaryButton_HoverTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));

            UpdateFreezable("SecondaryButton_PressedBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#94DCF7"));
            UpdateFreezable("SecondaryButton_PressedBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
            UpdateFreezable("SecondaryButton_PressedTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));

            UpdateFreezable("SecondaryButton_DisabledBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00FFFFFF"));
            UpdateFreezable("SecondaryButton_DisabledBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A3A3A3"));
            UpdateFreezable("SecondaryButton_DisabledTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
            #endregion

            #region SplitListView - EzBtn (Robert_Lin, 2024-10-24)
            UpdateFreezable("EzBtn_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("EzBtn_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#B6B6B6"));

            UpdateFreezable("EzBtn_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F0F0"));
            UpdateFreezable("EzBtn_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F0F0"));
            UpdateFreezable("EzBtn_BdColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#787878"));

            UpdateFreezable("EzBtn_Arrow_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("EzBtn_Arrow_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("EzBtn_Arrow_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            #endregion

            #region SplitItem of PIP/PBP
            UpdateFreezable("BorderColor_SplitNormal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
            #endregion

            #region SplitItem of EasyArrange
            UpdateFreezable("SplitCtrl_OuterBorder_BdColor_Icon_Normal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("SplitSplitter_Icon", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("SplitCtrl0B_IconFill", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("SplitItem_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
            #endregion

            #region LockIcon
            UpdateFreezable("LockIcon_Fill", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
            #endregion

            #region Progress Bar
            UpdateFreezable("ProgressBar_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
            UpdateFreezable("Unpair_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("WalkThroughBox_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("EQLine_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
            #endregion

            #region Grid Background
            UpdateFreezable("DefaultTheme_USBKVMPCGridBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
            UpdateFreezable("DefaultTheme_GridBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
            #endregion

            #region Setup Border Background
            UpdateFreezable("DefaultTheme_SetupScanBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F0F0"));
            UpdateFreezable("DefaultTheme_SetupScanBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("DefaultTheme_SetupSelectBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
            #endregion

            UpdateFreezable("DefaultThemeFullPageBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));


            BitmapImageUpdated?.Invoke(OSThemeEnum.Light);
        }
    }
}
