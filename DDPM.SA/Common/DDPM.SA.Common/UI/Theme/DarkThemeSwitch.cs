using DdmLibrary.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace DDPM.SA.Common.UI
{
    public static partial class SAUICommonHelper
    {
        public static void SwitchToDarkMode() 
        {
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_Border_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D9141D28"));
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Path_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround_1", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Select_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
            UpdateFreezable<SolidColorBrush>("Button_UXStyleColor_ForWhiteFrame", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable<SolidColorBrush>("PartBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable<SolidColorBrush>("Border_Hover_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_CameraSetting_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A0E14"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Menu_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Dragging_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#839DB4"));
            UpdateFreezable("TextBlock_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

            #region EA Dialog Styles
            UpdateFreezable<SolidColorBrush>("Dlg_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF293B4D"));
            UpdateFreezable<SolidColorBrush>("Dlg_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F20D121A"));
            UpdateFreezable<SolidColorBrush>("Dlg_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));

            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CCCCCC"));
            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#33444E"));
            #endregion EA Dialog Styles

            #region TextBox Robert_Lin, 2024-12-1
            UpdateFreezable("TextBox_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable("TextBox_Select", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("TextBox_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("TextBox_TextColor_Select", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("TextBox_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));
            UpdateFreezable("TextBox_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CACACA"));
            UpdateFreezable("TextBox_BdColor_Active", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#DAF5FD"));
            UpdateFreezable("TextBox_BdColor_Invalid", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF3E3B"));
            #endregion TextBox Robert_Lin, 2024-12-1

            #region Editable ComboBox
            UpdateFreezable("DdpmCB_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
            UpdateFreezable("DdpmCB_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
            UpdateFreezable("DdpmCB_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF232D36"));
            UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF232D36"));
            #endregion Editable ComboBox

            #region PrimaryButton
            UpdateFreezable("PrimaryButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("PrimaryButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable("PrimaryButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
            UpdateFreezable("PrimaryButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F2E3034"));

            UpdateFreezable("PrimaryButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("PrimaryButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("PrimaryButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("PrimaryButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F7C7C7C"));
            #endregion PrimaryButton

            #region SecondaryButton
            UpdateFreezable("SecondaryButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));
            UpdateFreezable("SecondaryButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
            UpdateFreezable("SecondaryButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
            UpdateFreezable("SecondaryButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));

            UpdateFreezable("SecondaryButton_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("SecondaryButton_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("SecondaryButton_BdColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("SecondaryButton_BdColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A3A3A3"));

            UpdateFreezable("SecondaryButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("SecondaryButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("SecondaryButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("SecondaryButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));

            #endregion

            #region DDPM.SA.Common.Popup.PopupBase Robert_Lin, 2024-12-2
            UpdateFreezable("popupBase_Window_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F20D121A"));
            UpdateFreezable("popupBase_Window_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
            UpdateFreezable("popupBase_CloseX_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CCCCCC"));
            UpdateFreezable("popupBase_Header_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("popupBase_Body_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            #endregion DDPM.SA.Common.Popup.PopupBase

            #region Tooltip
            UpdateFreezable("Tooltip_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF1D2C3B"));
            UpdateFreezable("Tooltip_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF293B4D"));
            UpdateFreezable("Tooltip_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));
            #endregion

            #region QAM Page
            UpdateBitmapImage("QAMImage_CallDDPM", new Uri($"pack://application:,,,/DDPM.QAM;component/Imgs/ddpm-dark.png", UriKind.RelativeOrAbsolute));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_CameraSetting_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Menu_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
            #endregion
        }
    }
}
