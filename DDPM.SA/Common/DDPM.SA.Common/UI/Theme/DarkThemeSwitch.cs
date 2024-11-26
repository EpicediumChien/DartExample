using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DDPM.SA.Common.UI
{
    public static partial class SACommonHelper
    {
        public static void SwitchToDarkMode() 
        {
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_Border_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Path_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Path_IsTrigger_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround_1", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Select_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
            UpdateFreezable<SolidColorBrush>("Button_UXStyleColor_ForWhiteFrame", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable<SolidColorBrush>("PartBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable<SolidColorBrush>("Border_Hover_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_CameraSetting_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A0E14"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Menu_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable("TextBlock_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

            #region EA Dialog Styles
            UpdateFreezable<SolidColorBrush>("Dlg_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF293B4D"));
            UpdateFreezable<SolidColorBrush>("Dlg_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F20D121A"));
            UpdateFreezable<SolidColorBrush>("Dlg_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));

            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CCCCCC"));
            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#33444E"));
            #endregion EA Dialog Styles
        }
    }
}
