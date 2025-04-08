using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DDPM.SA.Common.Popup
{
    public class PopupBaseManage
    {
        private static List<PopupBase>? _notificationWindows;
        private const double NotificationHeight = 252;
        private const double NotificationWidth = 417;
        private const double NotificationSpacing = 10;

        public event EventHandler<object> LeftButtonClick;

        public event EventHandler<object> RightButtonClick;

        public event EventHandler<object> Default_Event;

        public PopupBaseManage()
        {
            if (_notificationWindows == null)
            {
                _notificationWindows = new List<PopupBase>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="title"></param>
        /// <param name="info"></param>
        /// <param name="LeftButtonContent"></param>
        /// <param name="RightButtonContent"></param>
        /// <param name="ob">The object returned by the button event</param>
        /// <param name="stayOpen"></param>
        /// <param name="timeout"></param>
        public void FWU_Show(string title, string info, string LeftButtonContent, string RightButtonContent, object ob, bool stayOpen = false, int timeout = 5)
        {
            Thread thread1 = new Thread(() =>
            {
                PopupBase popupBase = new PopupBase(title, info, LeftButtonContent, RightButtonContent, ob, stayOpen, timeout);
                popupBase.LeftButtonClick += LeftButtonClick;
                popupBase.RightButtonClick += RightButtonClick;
                popupBase.Default_Event += Default_Event;
                popupBase.Height = NotificationHeight;
                popupBase.Width = NotificationWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                // Calculate the position of the new notification window
                int windowsPerColumn = (int)(screenHeight / (NotificationHeight + NotificationSpacing));
                int column = _notificationWindows.Count / windowsPerColumn;
                int row = _notificationWindows.Count % windowsPerColumn;
                double leftOffset = column * (NotificationWidth + NotificationSpacing);
                double bottomOffset = row * (NotificationHeight + NotificationSpacing);
                popupBase.Left = screenWidth - NotificationWidth - NotificationSpacing;
                popupBase.Top = screenHeight - NotificationHeight - NotificationSpacing - bottomOffset;
                // popupBase.Show();
                popupBase.ShowDialog();
                _notificationWindows.Add(popupBase);

                // Remove closed notification window
                popupBase.Closed += (s, e) => { 
                    _notificationWindows.Remove(popupBase);

                    //Derek 2025/03/31
                    if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
                    {
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                    }
                };
                Dispatcher.Run();
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
        }
    }
}